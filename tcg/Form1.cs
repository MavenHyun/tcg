using System;

using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace tcg
{
    public partial class Form1 : Form
    {
        public Socket sck;
        public EndPoint epLocal, epRemote;
        System.Windows.Forms.Timer moveTimer = new System.Windows.Forms.Timer();

        int select;
        bool initial = true;
        bool home_away = false;
 
        Stack<Pitch> m_deck = new Stack<Pitch>(); /*메인덱*/
        List<Card> b_deck = new List<Card>(); /*밴치덱*/
        Queue<Fielder> p_deck = new Queue<Fielder>(); /*선수덱*/
        Stack<Pitch> r_deck = new Stack<Pitch>(); /*예비덱*/
        List<Pitch> hand = new List<Pitch>(); /*핸드*/
        Stack<Pitcher> p_zone = new Stack<Pitcher>(); /*투수존*/
        Stack<Fielder> b_zone = new Stack<Fielder>(); /*타자존*/
        Fielder[] f_holder = new Fielder[10]; /*포지션*/
        Stack<Pitch> d_pile = new Stack<Pitch>(); /*버림모음*/

        bool[,] s_zone = new bool[7, 7]; /*스트라이크존*/
        int[,] d_zone = new int[3, 3]; /*야수존*/
        bool[,] zone = new bool[3, 3]; /*피칭존*/
        bool[,] h_zone = new bool[3, 3]; /*히팅존*/
        string[] bases = new string[4]; /*베이스*/

        Pitch incoming; /*들어오는 피치*/
        Fielder defender; /*타구를 처리하는야수*/
        Situation Helper = new Situation(); /*Helper*/

        //bool inning = true; //true: Top, false: Bottom

        public Form1()
        {
            InitializeComponent();
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            Board.load_Main(m_deck);
            Board.load_Player(p_deck, p_zone);
            Board.load_Bench(b_deck);
            Board.load_Reserve(r_deck);
            Board.load_Manager(pb_manager);

            ip_you.Text = "25.68.229.111";
            port_you.Text = "1111";
            ip_opp.Text = "25.68.229.111";
            port_opp.Text = "1111";

            game_text.Text = "당신은 갓동님 입니다!\r\n";
            game_text.Text += "게임을 시작하려면 '게임 시작'을 누르세요!\r\n";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            try
            {
                btn_start.Enabled = true;
                btn_connect.Enabled = false;
                epLocal = new IPEndPoint(IPAddress.Parse(ip_you.Text), Convert.ToInt32(port_you.Text));
                sck.Bind(epLocal);

                epRemote = new IPEndPoint(IPAddress.Parse(ip_opp.Text), Convert.ToInt32(port_opp.Text));
                sck.Connect(epRemote);

                byte[] buffer = new byte[10000];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                btn_connect.Visible = false;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            Label[] f = { Pitcher, Catcher, firstBase, secondBase, thirdBase, shortStop, leftFielder, centerFielder, rightFielder };
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);
                if (size > 0)
                {
                    UTF8Encoding eEncoding = new UTF8Encoding();
                    JavaScriptSerializer json = new JavaScriptSerializer();
                    byte[] receivedData = new byte[10000];
                    receivedData = (byte[])aResult.AsyncState;

                    byte[] receivedDataFormatted = new byte[size];
                    for(int i=0; i < size; i++)
                    {
                        receivedDataFormatted[i] = receivedData[i];
                    }

                    string receivedMessage = eEncoding.GetString(receivedDataFormatted);
                    Helper = json.Deserialize<Situation>(receivedMessage);
                    Helper.SwapRuns();
                    CheckForIllegalCrossThreadCalls = false;
                    Helper.start_game = true;
                    btn_start.Enabled = false;
                    btn_start.Visible = false;

                    if (initial)
                    {
                        initial = false;
                        game_text.Text = "잠시만 기다려주세요...";
                    }

                    if (Helper.start_game)
                    {
                        if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
                        {//Defense
                            if (Helper.go_inning)
                            {
                                Helper.go_inning = false;
                                Board.load_Batter(Helper.b_id,Helper.b_bats, pb_lhb, pb_rhb);
                                phase_atbat();
                            }

                            if (Helper.go_atbat0)
                            {
                                Helper.go_atbat0 = false;
                                phase_atbat();
                            }

                            if (Helper.go_atbat)
                            {
                                Helper.go_atbat = false;
                                if (select == Helper.choose)
                                {
                                    phase_pitch();
                                }
                            }

                            if (Helper.go_result)
                            {
                                Helper.go_result = false;
                                phase_result();
                            }
                        }
                        else
                        {//Offense
                            if (Helper.go_inning)
                            {
                                phase_inning();

                                Board.Shuffle(m_deck); Board.Draw(m_deck, hand); Board.Draw(m_deck, hand);
                                Board.Draw(m_deck, hand); Board.Draw(m_deck, hand);
                                Board.showHand(ph, hand); Board.load_StrikeZone(sz);
                                Board.load_Fielders(f, Helper.f_holder); Board.load_Pitcher(Helper.p_id, pb_pitcher);
                                Board.load_Batter(p_deck, pb_lhb, pb_rhb);

                                Helper.b_id = p_deck.First().card_id;
                                Helper.b_name = p_deck.First().card_name;
                                Helper.b_con = p_deck.First().card_con;
                                Helper.b_bats = p_deck.First().card_bats;
                                Board.send(Helper, sck);
                            }
                            if (Helper.go_atbat)
                            {
                                phase_atbat();
                                Board.load_StrikeZone(sz);
                                Button[] bs = { btn_batting, btn_ph, btn_pr };
                                Board.comp_enable(bs);
                                game_text.Text = "타석 단계 입니다. 배팅을 눌러주세요\r\n";
                            }

                            if (Helper.go_sign)
                            {
                                Helper.go_sign = false;
                                moveTimer.Interval = (60 * 1000);
                                moveTimer.Tick += new EventHandler(tick_off);
                                moveTimer.Start();

                                game_text.Text = "예측 단계\r\n";
                                game_text.Text = "상대 투수가 던질 것 같은 구종을 패에서 하나 골라 클릭하세요!\r\n";
                                Board.comp_enable(ph);
                            }
                            if (Helper.go_final)
                            {
                                Helper.go_final = false;
                                Helper.go_atbat0 = true;
                                game_text.Text = Helper.game_msg;
                                Helper.SaveBase(bases);
                                Board.send(Helper, sck);
                            }
                            if (Helper.go_final2)
                            {
                                Helper.go_final2 = false;
                                Board.next_Batter(p_deck, pb_lhb, pb_rhb);
                                Board.load_Batter(p_deck, pb_lhb, pb_rhb);
                                Helper.go_atbat0 = true;
                                game_text.Text = Helper.game_msg;
                                Helper.SaveBase(bases);
                                Board.send(Helper, sck);
                            }
                            if (Helper.go_final3)
                            {
                                Helper.go_final3 = false;
                                Board.next_Batter(p_deck, pb_lhb, pb_rhb);
                                Board.merge_Decks(m_deck, d_pile, hand);
                                Helper.inning = (!Helper.inning);
                                phase_inning();
                            }
                        }
                    }
                }
                byte[] buffer = new byte[10000];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);         
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void phase_inning()
        {//이닝 시작
            Helper.init = false;
            Label[] f = {Pitcher, Catcher, firstBase, secondBase, thirdBase, shortStop, leftFielder, centerFielder, rightFielder};
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            Button[] bc = { btn_proceed, btn_ph, btn_pr, btn_batting, btn_psub, btn_fsub, btn_pitching, btn_wait, btn_cut, btn_swing };
            PictureBox[] home = { home1, home2, home3, home4, home5, home6, home7, home8, home9 };
            PictureBox[] away = { away1, away2, away3, away4, away5, away6, away7, away8, away9 };
            PictureBox[] rhb_home = { home_run, home_hit, home_walk };
            PictureBox[] rhb_away = { away_run, away_hit, away_walk };
            PictureBox[] osb = { out_count, strike_count, ball_count };

            Board.init_zone(h_zone);
            Board.init_zone(zone);
            Board.init_zone2(s_zone);
            Board.init_zone(d_zone);

            Board.comp_disable(sz);
            Board.comp_disable(ph);
            Board.comp_disable(bc);

            Board.load_Score(rhb_away, rhb_home, osb, away, home, Helper);

            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {//Defense
                Board.Shuffle(m_deck); Board.Draw(m_deck, hand); Board.Draw(m_deck, hand);
                Board.Draw(m_deck, hand); Board.Draw(m_deck, hand); 
                Board.showHand(ph, hand); Board.load_StrikeZone(sz, ref d_zone);
                Board.load_Fielders(f, f_holder, p_zone); Board.load_Pitcher(p_zone, pb_pitcher);
                
                Helper.go_inning = true;
                Helper.SaveFH(f_holder, p_zone);
                Helper.p_id = p_zone.Peek().card_id;
                Helper.p_name = p_zone.Peek().card_name;
                Board.send(Helper, sck);
            }
        }

        private void phase_atbat()
        {//선수 지시 단계
            if (m_deck.Count == 0)
            {
                Board.merge_Decks(m_deck, d_pile, hand);
                Board.Draw(m_deck, hand);
                Board.Draw(m_deck, hand);
            }
            Board.Draw(m_deck, hand);
            if (Helper.bonus)
            {
                Board.Draw(m_deck, hand);
            }
            Helper.ResetValues();

            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
            Button[] bs = { btn_proceed, btn_ph, btn_pr, btn_batting, btn_psub, btn_fsub, btn_pitching, btn_wait, btn_cut, btn_swing };
            Button[] bs2 = { btn_psub, btn_fsub, btn_pitching };
            PictureBox[] b = { base1, base2, base3 };
            PictureBox[] away = { away1, away2, away3, away4, away5, away6, away7, away8, away9 };
            PictureBox[] home = { home1, home2, home3, home4, home5, home6, home7, home8, home9 };
            PictureBox[] rhb_away = { away_run, away_hit, away_walk };
            PictureBox[] rhb_home = { home_run, home_hit, home_walk };
            PictureBox[] osb = { out_count, strike_count, ball_count };
            
            Board.showHand(ph, hand);
            Board.load_Score(rhb_away, rhb_home, osb, away, home, Helper);

            Board.comp_disable(ph);
            Board.comp_disable(sz);
            Board.comp_disable(bs);

            Board.init_zone(zone);
            Board.init_zone(h_zone);
            Board.init_zone2(s_zone);
            Board.load_Bases(bases, b);
            
            if (Helper.game_end)
            {
                game_finish();
            }
            else
            {
                if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
                {
                    pitches.Text = "현재 투구수: " + Helper.pitch_count;
                    Board.load_StrikeZone(sz, ref d_zone);
                    Board.comp_enable(bs2);
                    game_text.Text = "타석 단계 입니다. 피칭을 눌러주세요\r\n";
                }
                else
                {
                    pitches.Text = "현재 투구수: " + Helper.opp_pitch_count;
                }
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            btn_start.Enabled = false;
            btn_start.Visible = false;
            home_away = true;
            phase_inning();
        }
        private void btn_proceed_Click(object sender, EventArgs e)
        {
            if (Helper.init)
            {
                if (!Helper.go_final3)
                {
                    btn_proceed.Enabled = false;
                    phase_inning();
                }
                else
                {
                    Board.send(Helper, sck);
                }
            }
            else
            {
                Board.send(Helper, sck);
            }
        }
        private void btn_pitching_Click(object sender, EventArgs e)
        {
            Button[] bs = { btn_psub, btn_fsub, btn_pitching };
            Board.comp_disable(bs);
            select = 3;
            game_text.Text = "잠시만 기다려주세요...";

            Helper.choose = 3;
            Helper.go_atbat = true;
            Board.send(Helper, sck);
        }
        private void btn_batting_Click(object sender, EventArgs e)
        {
            Button[] bs = { btn_batting, btn_ph, btn_pr };
            Board.comp_disable(bs);

            Helper.choose = 3;
            Board.send(Helper, sck);
        }

        private void phase_pitch()
        {//구종 선택 단계
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            Board.comp_enable(ph);

            game_text.Text = "구종 단계\r\n";
            game_text.Text += "패에 있는 구종 카드를 골라 클릭하세요!\r\n";
            moveTimer.Interval = (60 * 1000);
            moveTimer.Tick += new EventHandler(tick_def);
            moveTimer.Start();
            //
            Helper.locked = false;
        }
        private void phase_sign()
        {// 작전 지시 단계
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            Board.comp_disable(ph);
            Button[] bs = { btn_wait, btn_swing, btn_cut };
            Board.comp_enable(bs);
            game_text.Text = "지시 단계\r\n";
            game_text.Text += "스윙, 커트, 웨이팅 중 하나 골라 클릭하세요!\r\n";
        }

        private void hand1_Click(object sender, EventArgs e)
        {
            d_pile.Push(hand.ElementAt(0));
            hand.RemoveAt(0);
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            Board.showHand(ph, hand);
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                if (!Helper.locked)
                {
                    Helper.locked = true;
                    incoming = d_pile.Peek();
                    phase_loc();
                }
                else
                {
                    incoming.card_cost--;
                    phase_cost();
                }
            }
            else
            {
                Board.comp_disable(ph);
                Helper.d_type = d_pile.Peek().card_type;
                phase_sign();
            }
        }
        private void hand2_Click(object sender, EventArgs e)
        {
            d_pile.Push(hand.ElementAt(1));
            hand.RemoveAt(1);
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            Board.showHand(ph, hand);
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                if (!Helper.locked)
                {
                    Helper.locked = true;
                    incoming = d_pile.Peek();
                    phase_loc();
                }
                else
                {
                    incoming.card_cost--;
                    phase_cost();
                }
            }
            else
            {
                Board.comp_disable(ph);
                Helper.d_type = d_pile.Peek().card_type;
                phase_sign();
            }
        }
        private void hand3_Click(object sender, EventArgs e)
        {
            d_pile.Push(hand.ElementAt(2));
            hand.RemoveAt(2);
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            Board.showHand(ph, hand);
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                if (!Helper.locked)
                {
                    Helper.locked = true;
                    incoming = d_pile.Peek();
                    phase_loc();
                }
                else
                {
                    incoming.card_cost--;
                    phase_cost();
                }
            }
            else
            {
                Board.comp_disable(ph);
                Helper.d_type = d_pile.Peek().card_type;
                phase_sign();
            }
        }
        private void hand4_Click(object sender, EventArgs e)
        {
            d_pile.Push(hand.ElementAt(3));
            hand.RemoveAt(3);
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            Board.showHand(ph, hand);
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                if (!Helper.locked)
                {
                    Helper.locked = true;
                    incoming = d_pile.Peek();
                    phase_loc();
                }
                else
                {
                    incoming.card_cost--;
                    phase_cost();
                }
            }
            else
            {
                Board.comp_disable(ph);
                Helper.d_type = d_pile.Peek().card_type;
                phase_sign();
            }
        }
        private void hand5_Click(object sender, EventArgs e)
        {
            d_pile.Push(hand.ElementAt(4));
            hand.RemoveAt(4);
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            Board.showHand(ph, hand);
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                if (!Helper.locked)
                {
                    Helper.locked = true;
                    incoming = d_pile.Peek();
                    phase_loc();
                }
                else
                {
                    incoming.card_cost--;
                    phase_cost();
                }
            }
            else
            {
                Board.comp_disable(ph);
                Helper.d_type = d_pile.Peek().card_type;
                phase_sign();
            }
        }

        private void tick_off(object sender, EventArgs e)
        {
            moveTimer.Stop();
            btn_wait.Enabled = false;
            btn_cut.Enabled = false;
            btn_swing.Enabled = false;
            game_text.Text += "시간 초과... 비바뭐시기!!!\r\n";
            Helper.choose = 1;
            Helper.go_result = true;
            Board.send(Helper, sck);
        }

        private void btn_swing_Click(object sender, EventArgs e)
        {
            moveTimer.Stop();
            Button[] bs = { btn_wait, btn_swing, btn_cut };
            Board.comp_disable(bs);
            Helper.go_result = true;
            Helper.choose = 3;
            phase_contact();
        }
        private void btn_cut_Click(object sender, EventArgs e)
        {
            moveTimer.Stop();
            Button[] bs = { btn_wait, btn_swing, btn_cut };
            Board.comp_disable(bs);
            Helper.go_result = true;
            Helper.choose = 2;
            Board.send(Helper, sck);
            game_text.Text = "잠시만 기다려주세요...\r\n";
        }
        private void btn_wait_Click(object sender, EventArgs e)
        {
            moveTimer.Stop();
            Button[] bs = { btn_wait, btn_swing, btn_cut };
            Board.comp_disable(bs);
            Helper.go_result = true;
            Helper.choose = 1;
            Board.send(Helper, sck);
            game_text.Text = "잠시만 기다려주세요...\r\n";
        }

        private void phase_contact()
        {
            Helper.cost = p_deck.Peek().card_con;
            game_text.Text = "컨택 단계\r\n";
            game_text.Text += "스트존 뒷면 카드 9장 중에 하나를 골라 클릭하세요!\r\n";
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
            Board.comp_disable(ph);
            Board.comp_enable(sz);
            Board.copy_zone_off(zone, Helper);
            Board.show_loc(zone, sz);
        }
        private void phase_loc()
        {//로케이션 선택 단계
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {//Defense
                game_text.Text = "로케이션 단계\r\n";
                game_text.Text += "스트존 뒷면 카드 9장 중에 하나를 골라 클릭하세요!\r\n";
                PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
                PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
                Board.comp_disable(ph);
                Board.comp_enable(sz);
            }
            else
            {
                Helper.cost = p_deck.Peek().card_con;
            }
        }

        private void tick_def(object sender, EventArgs e)
        {
            moveTimer.Stop();
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz43 };
            game_text.Text += "시간 초과... 실투!!!\r\n";
            Pitch bad = new Pitch("실투", 0, "0", 0, 0, 0);
            incoming = bad;
            Board.comp_disable(sz);
            Board.init_zone(zone);
            zone[1, 1] = true;

            Helper.go_sign = true;
            Board.copy_zone_def(zone, Helper);
            Board.send(Helper, sck);
        }

        private void sz22_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[0, 0] = true;
                sz22.Image = null;
                sz22.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[0, 0] = true;
                sz22.Image = null;
                sz22.BackColor = Color.Azure;
                sz22.Enabled = false;
                phase_contact2();
            }
        }
        private void sz23_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[0, 1] = true;
                sz23.Image = null;
                sz23.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[0, 1] = true;
                sz23.Image = null;
                sz23.BackColor = Color.Azure;
                sz23.Enabled = false;
                phase_contact2();
            }
        }
        private void sz24_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[0, 2] = true;
                sz24.Image = null;
                sz24.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[0, 2] = true;
                sz24.Image = null;
                sz24.BackColor = Color.Azure;
                sz24.Enabled = false;
                phase_contact2();
            }
        }
        private void sz32_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[1, 0] = true;
                sz32.Image = null;
                sz32.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[1, 0] = true;
                sz32.Image = null;
                sz32.BackColor = Color.Azure;
                sz32.Enabled = false;
                phase_contact2();
            }
        }
        private void sz33_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[1, 1] = true;
                sz33.Image = null;
                sz33.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[1, 1] = true;
                sz33.Image = null;
                sz33.BackColor = Color.Azure;
                sz33.Enabled = false;
                phase_contact2();
            }
        }
        private void sz34_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[1, 2] = true;
                sz34.Image = null;
                sz34.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[1, 2] = true;
                sz34.Image = null;
                sz34.BackColor = Color.Azure;
                sz34.Enabled = false;
                phase_contact2();
            }
        }
        private void sz42_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[2, 0] = true;
                sz42.Image = null;
                sz42.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[2, 0] = true;
                sz42.Image = null;
                sz42.BackColor = Color.Azure;
                sz42.Enabled = false;
                phase_contact2();
            }
        }
        private void sz43_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[2, 1] = true;
                sz43.Image = null;
                sz43.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[2, 1] = true;
                sz43.Image = null;
                sz43.BackColor = Color.Azure;
                sz43.Enabled = false;
                phase_contact2();
            }
        }
        private void sz44_Click(object sender, EventArgs e)
        {
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                zone[2, 2] = true;
                sz44.Image = null;
                sz44.BackColor = Color.Azure;
                phase_cost();
            }
            else
            {
                h_zone[2, 2] = true;
                sz44.Image = null;
                sz44.BackColor = Color.Azure;
                sz44.Enabled = false;
                phase_contact2();
            }
        }

        private void phase_contact2()
        {//컨택존 선택
            game_text.Text = "현재 남은 컨택존 수 : " + Helper.cost + "\r\n";
            game_text.Text += "준비를 마쳤으면 타자 카드를 클릭하세요!\r\n";
            Helper.cost--;
            Helper.bp_mod = p_deck.Peek().card_bp + Helper.cost;
            if (p_deck.Peek().card_bats == Constants.LEFT)
            {
                pb_lhb.Enabled = true;
            }
            else
            {
                pb_rhb.Enabled = true;
            }
            if (Helper.cost == 0)
            {
                PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
                Board.comp_disable(sz);
            }
        }
        private void LEFT_BATTER_Click(object sender, EventArgs e)
        {
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
            Board.comp_disable(sz);
            pb_lhb.Enabled = false;
            game_text.Text = "잠시만 기다리세요...";
            Board.copy_zone_def(h_zone, Helper);
            Helper.go_result = true;
            Helper.b_con = p_deck.Peek().card_con;
            Board.send(Helper, sck);
        }
        private void RIGHT_BATTER_Click(object sender, EventArgs e)
        {
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
            Board.comp_disable(sz);
            pb_rhb.Enabled = false;
            game_text.Text = "잠시만 기다리세요...";
            Board.copy_zone_def(h_zone, Helper);
            Helper.go_result = true;
            Helper.b_con = p_deck.Peek().card_con;
            Board.send(Helper, sck);
        }

        private void phase_cost()
        {
            game_text.Text = "변화구 코스트 지불\r\n";
            game_text.Text += "현재 남은 비용 카드 수는" + incoming.card_cost + "\r\n";
            PictureBox[] ph = { hand1, hand2, hand3, hand4, hand5 };
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz43 };
            Board.comp_disable(sz);
            if (incoming.card_cost != 0)
            {
                if (incoming.card_cost <= hand.Count())
                {
                    btn_proceed.Enabled = false;
                    for (int i = 0; i < hand.Count; i++)
                    {
                        ph[i].Enabled = true;
                    }
                    for (int i = hand.Count; i < 5; i++)
                    {
                        ph[i].Enabled = false;
                    }
                }
                else
                {
                    game_text.Text += "비용 부족... 실투!!!\r\n";
                    Pitch bad = new Pitch("실투", 0, "0", 0, 0, 0);
                    incoming = bad;
                    Board.comp_disable(ph);
                    Board.init_zone(zone);
                    zone[1, 1] = true;
                    moveTimer.Stop();
                    Helper.go_sign = true;
                    Board.copy_zone_def(zone, Helper);
                    game_text.Text = "잠시만 기다리세요...";
                    Board.send(Helper, sck);
                }
            }
            else
            {
                moveTimer.Stop();
                Board.showHand(ph, hand);
                Board.comp_disable(ph);
                Helper.go_sign = true;
                Board.copy_zone_def(zone, Helper);
                game_text.Text = "잠시만 기다리세요...";
                Board.send(Helper, sck);
            }
        }

        private void phase_result()
        {
            if (Helper.choose == 3)
            {
                Board.copy_zone_off(h_zone, Helper);
                phase_result3();
            }
            else if (Helper.choose == 2)
            {
                phase_result2();
            }
            else
            {
                phase_result1();
            }
        }
        private void phase_result1()
        {//웨이팅시 결과 단계
            int x = 0;
            int y = 0;
            Helper.game_msg += "마운드에 있는 " + Helper.p_name + " 선수는 " + incoming.card_name + "을 던집니다!\r\n";
            Helper.game_msg += "타석에 있는 " + Helper.b_name + " 선수는 배트를 내지 않고 기다립니다.\r\n";
            Board.pitch_move(zone, incoming, p_zone, ref x, ref y);
            Helper.pitch_count++;
            if (incoming.card_type.Equals(Helper.d_type))//수정 요망!
            {
                Helper.bonus = false;
            }
            else
            {
                Helper.bonus = true;
            }
            if (s_zone[x, y])
            {
                Helper.strike++;
                Helper.game_msg += "스트라이크 입니다!\r\n";
            }
            else
            {
                Helper.ball++;
                Helper.game_msg += "볼 입니다!\r\n";
            }
            phase_final();
        }
        private void phase_result2()
        {//커트시 결과 단계
            if ((home_away && Helper.inning) || ((!home_away) && (!Helper.inning)))
            {
                Helper.game_msg += "마운드에 있는 " + Helper.p_name + " 선수는 " + incoming.card_name + "을 던집니다!\r\n";
                Helper.game_msg += "타석에 있는 " + Helper.b_name + " 선수는 공을 걷어내려고 하는데\r\n";
                Helper.pitch_count++;
                if (incoming.card_type.Equals(Helper.d_type))//수정 요망!
                {
                    Helper.game_msg += "파울! 커트를 성공합니다!\r\n";
                    Helper.bonus = false;
                    if (Helper.strike < 2)
                    {
                        Helper.strike++; /*foul*/
                    }
                }
                else
                {
                    Helper.game_msg += "체크 스윙... 스트라이크 판정입니다!\r\n";
                    Helper.bonus = true;
                    Helper.strike++; /*check-swing*/
                }
            }
            phase_final();
        }
        private void phase_result3()
        {//스윙시 결과 단계
            Helper.game_msg += "마운드에 있는 " + Helper.p_name + " 선수는 " + incoming.card_name + "을 던집니다!\r\n";
            Helper.game_msg += "타석에 있는 " + Helper.b_name + " 선수는 타격 자세를 취하는데요...\r\n";
            int x = 0;
            int y = 0;
            int a = 0;
            int b = 0;
            PictureBox[] sz = { sz22, sz23, sz24, sz32, sz33, sz34, sz42, sz43, sz44 };
            Helper.pitch_count++;
            Board.pitch_move(zone, incoming, p_zone, ref x, ref y, ref a, ref b);

            if (s_zone[x, y])
            {
                defender = f_holder[d_zone[a, b]];
                Helper.def_pos = d_zone[a, b];
            }

            if (h_zone[a, b])
            {
                int acc = Helper.bp_mod - p_zone.Peek().card_pp;
                if (((acc > 0) && (acc < 2)))
                {//foul
                    Helper.game_msg += "파울!\r\n";
                    if (Helper.strike < 2)
                    {
                        Helper.strike++;
                    }
                    phase_final();
                }
                else if (acc <= 0)
                {//swing
                    Helper.game_msg += "스윙 스트라이크! 공을 따라가지 못합니다!\r\n";
                    Helper.strike++;
                    phase_final();
                }
                else
                {
                    Helper.game_msg += "쳤습니다..........!\r\n";
                    Board.show_fielder(f_holder, sz, d_zone, a, b);
                    phase_inplay();
                }
            }
            else
            {//swing
                Helper.game_msg += "스윙 스트라이크! 유인구에 속네요!\r\n";
                Helper.strike++;
                phase_final();
            }
        }
        private void phase_inplay()
        {//인플레이 단계
            int output = 0;

            output = (Helper.bp_mod + Helper.b_con) - (p_zone.Peek().card_pp + defender.card_def);

            if (output <= 0)
            {
                if (bases[1] == null)
                {//아웃
                    Helper.game_msg += "야수가 타구를 잘 처리하여 아웃 카운트를 하나 늘립니다!\r\n";
                    Helper.outs++;
                }
                else
                {
                    if (Helper.def_pos == 6 || Helper.def_pos == 5 || Helper.def_pos == 4)
                    {//병살
                        Helper.game_msg += "타구가 성흔합니다! 아웃! 아웃! 병살이네요!\r\n";
                        bases[1] = null;
                        Helper.outs += 2;
                    }
                }
                if (bases[3] != null)
                {
                    if (Helper.def_pos == 7 || Helper.def_pos == 8 || Helper.def_pos == 9)
                    {//희생플라이
                        Helper.game_msg += "타구가 잡히자 마자 3루 주자가 홈에 쇄도하여 득점합니다!\r\n";
                        Helper.game_msg += "3루 주자 홈인!\r\n";
                        bases[3] = null;
                        Helper.opp_runs++;
                        Helper.inning_runs++;
                    }
                }
            }
            else
            {
                Helper.game_msg += output + "루타 입니다!\r\n";
                Helper.opp_hits++;
                if (output < 4)
                {//안타
                    switch (output)
                    {
                        case 1://1루타
                            if (bases[3] != null)
                            {//3루 주자 홈인
                                Helper.game_msg += "3루 주자, 홈인!\r\n";
                                bases[3] = null;
                                Helper.inning_runs++;
                                Helper.opp_runs++;
                            }
                            if (bases[2] != null)
                            {//2루 주자 진루
                                Helper.game_msg += "2루 주자, 3루로 진루합니다!\r\n";
                                bases[3] = bases[2];
                                bases[2] = null;
                            }
                            if (bases[1] != null)
                            {//1루 주자 진루
                                Helper.game_msg += "1루 주자, 2루로 진루합니다!\r\n";
                                bases[2] = bases[1];
                                bases[1] = null;
                            }
                            bases[1] = Helper.b_name; //타자 주자 출루
                            break;
                        case 2://2루타
                            if (bases[3] != null)
                            {//3루 주자 홈인
                                Helper.game_msg += "3루 주자, 홈인!\r\n";
                                bases[3] = null;
                                Helper.inning_runs++;
                                Helper.opp_runs++;
                            }
                            if (bases[2] != null)
                            {//2루 주자 홈인
                                Helper.game_msg += "2루 주자, 홈인!\r\n";
                                bases[2] = null;
                                Helper.inning_runs++;
                                Helper.opp_runs++;
                            }
                            if (bases[1] != null)
                            {//1루 주자 진루
                                Helper.game_msg += "1루 주자, 3루로 진루합니다!\r\n";
                                bases[3] = bases[1];
                                bases[1] = null;
                            }
                            bases[2] = Helper.b_name; //타자 주자 출루
                            break;
                        case 3:
                            if (bases[3] != null)
                            {//3루 주자 홈인
                                Helper.game_msg += "3루 주자, 홈인!\r\n";
                                bases[3] = null;
                                Helper.inning_runs++;
                                Helper.opp_runs++;
                            }
                            if (bases[2] != null)
                            {//2루 주자 홈인
                                Helper.game_msg += "3루 주자, 홈인!\r\n";
                                bases[2] = null;
                                Helper.inning_runs++;
                                Helper.opp_runs++;
                            }
                            if (bases[1] != null)
                            {//1루 주자 홈인
                                Helper.game_msg += "1루 주자, 홈인!\r\n";
                                bases[3] = bases[1];
                                bases[1] = null;
                                Helper.inning_runs++;
                                Helper.opp_runs++;
                            }
                            bases[3] = Helper.b_name; //타자 주자 출루
                            break;
                        default:
                            break;
                    }
                }
                else
                {//홈런 
                    Helper.game_msg += "호무란~~~~~~~~~~~~~~~~~~~!\r\n";
                    Helper.opp_runs++;
                    Helper.inning_runs++;
                    if (bases[1] != null)
                    {//1루 주자 홈인
                        Helper.game_msg += "1루 주자, 홈인!\r\n";
                        bases[1] = null;
                        Helper.opp_runs++;
                        Helper.inning_runs++;
                    }
                    if (bases[2] != null)
                    {//2루 주자 홈인
                        Helper.game_msg += "2루 주자, 홈인!\r\n";
                        bases[2] = null;
                        Helper.opp_runs++;
                        Helper.inning_runs++;
                    }
                    if (bases[3] != null)
                    {//3루 주자 홈인
                        Helper.game_msg += "3루 주자, 홈인!\r\n";
                        bases[3] = null;
                        Helper.opp_runs++;
                        Helper.inning_runs++;
                    }

                }
            }
            phase_final2();
        }

        private void phase_final()
        {
            if (Helper.ball == 4)
            {//볼넷
                Helper.game_msg += "볼넷입니다!" + Helper.b_name + " 선수는 1루 베이스로 걸어 나갑니다!\r\n";
                if (bases[1] != null)
                {
                    if (bases[2] != null)
                    {
                        if (bases[3] != null)
                        {//밀어내기 볼넷
                            Helper.game_msg += "아... 밀어내기네요!";
                            bases[3] = bases[2];
                            bases[2] = bases[1];
                            Helper.opp_runs++;
                            Helper.inning_runs++;
                        }
                        else
                        {
                            bases[3] = bases[2];
                            bases[2] = bases[1];
                        }
                    }
                    else
                    {
                        bases[2] = bases[1];
                    }
                }
                bases[1] = Helper.b_name;
                Helper.opp_walks++;
                phase_final2();
            }
            else if (Helper.strike == 3)
            {//삼진
                Helper.game_msg += "스트라이크 아웃!" + Helper.b_name + " 선수는 삼진으로 물러납니다!\r\n";
                Helper.outs++;
                phase_final2();
            }
            else
            {//다음 투구
                Helper.go_final = true;
                Helper.SaveBase(bases);
                game_text.Text = Helper.game_msg;
                game_text.Text += "계속하려면 진행을 누르세요!";
                btn_proceed.Enabled = true;
            }
        }
        private void phase_final2()
        {
            if (Helper.outs != 3)
            {//다음 타자
                Helper.ball = 0;
                Helper.strike = 0;
                Helper.go_final2 = true;
                Helper.SaveBase(bases);
                game_text.Text = Helper.game_msg;
                game_text.Text += "다음 타자가 타석에 들어옵니다!\r\n";
                game_text.Text += "계속하려면 진행을 누르세요!";
                btn_proceed.Enabled = true;
            }
            else 
            {//공수 교대
                Helper.ball = 0;
                Helper.strike = 0;
                Helper.outs = 0;
                Helper.init = true;
                Helper.go_final3 = true;
                game_text.Text = Helper.game_msg;
                game_text.Text += "공수교대 합니다!\r\n";
                game_text.Text += "계속하려면 진행을 누르세요!";
                Board.merge_Decks(m_deck, d_pile, hand);
                bases[1] = null; bases[2] = null; bases[3] = null;
                btn_proceed.Enabled = true;
                if ((Helper.inning_count == 9) && (!Helper.inning))
                {
                    Helper.game_end = true;
                }
            }
        }

        private void game_finish()
        {
            game_text.Text = "경기 종료! 지금까지 게임을 해주셔서 진심으로 감사드립니다!" + "\r\n";
            game_text.Text = "지금 보이는 화면을 캡쳐 찍어서 페북에 인증하시면 즉시 선물을 보내드리겠습니다!";
        }
    }

    public static class Board
    {
        public static void Shuffle<Pitch>(this Stack<Pitch> deck)
        {
            Random rnd = new Random();
            var values = deck.ToArray();
            deck.Clear();
            foreach (var value in values.OrderBy(x => rnd.Next()))
                deck.Push(value);
        }
        public static void Shuffle(this Stack<int> deck)
        {
            Random rnd = new Random();
            var values = deck.ToArray();
            deck.Clear();
            foreach (var value in values.OrderBy(x => rnd.Next()))
                deck.Push(value);
        }
        public static void load_Main(this Stack<Pitch> d1)
        {
            string fnp_db = "resource/card_pitch.xml";
            string fnp_p1 = "resource/p1_maindeck.xml";

            XmlReader p1;
            XmlReader db;

            try
            {
                XmlReaderSettings setting = new XmlReaderSettings();
                setting.IgnoreComments = true;
                setting.IgnoreWhitespace = true;

                db = XmlReader.Create(fnp_db, setting);
                p1 = XmlReader.Create(fnp_p1, setting);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            while (p1.Read())
            {
                if (p1.Name.CompareTo("Item") == 0 && p1.NodeType == XmlNodeType.Element)
                {
                    int id1 = int.Parse(p1.GetAttribute("card_id"));
                    int num1 = int.Parse(p1.GetAttribute("num"));

                    while (db.Read())
                    {
                        if (db.Name.CompareTo("Item") == 0 && db.NodeType == XmlNodeType.Element)
                        {
                            int id = int.Parse(db.GetAttribute("card_id"));
                            if (id1 == id)
                            {
                                string name1 = db.GetAttribute("card_name");
                                string type = db.GetAttribute("card_type");
                                int cost = int.Parse(db.GetAttribute("card_cost"));
                                int move = int.Parse(db.GetAttribute("card_move"));
                                int effect = int.Parse(db.GetAttribute("card_effect"));

                                for (int i = 0; i < num1; i++)
                                {
                                    Pitch card = new Pitch(name1, id1, type, cost, move, effect);
                                    d1.Push(card);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            db.Close();
            p1.Close();
        }
        public static void load_Player(this Queue<Fielder> d1, Stack<Pitcher> c1)
        {
            XmlReader db;
            XmlReader r1;

            string fnp_db = "resource/card_player.xml";
            string fnp_p1 = "resource/p1_playerdeck.xml";

            try
            {
                XmlReaderSettings setting = new XmlReaderSettings();
                setting.IgnoreComments = true;
                setting.IgnoreWhitespace = true;

                r1 = XmlReader.Create(fnp_p1, setting);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            while (r1.Read())
            {
                if (r1.Name.CompareTo("Item") == 0 && r1.NodeType == XmlNodeType.Element)
                {
                    int id4 = int.Parse(r1.GetAttribute("card_id"));

                    try
                    {
                        XmlReaderSettings setting = new XmlReaderSettings();
                        setting.IgnoreComments = true;
                        setting.IgnoreWhitespace = true;

                        db = XmlReader.Create(fnp_db, setting);
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return;
                    }

                    while (db.Read())
                    {
                        if (db.Name.CompareTo("Item") == 0 && db.NodeType == XmlNodeType.Element)
                        {
                            int id3 = int.Parse(db.GetAttribute("card_id"));
                            if (id3 == id4)
                            {
                                string name = db.GetAttribute("card_name");
                                string team = db.GetAttribute("card_team");
                                int season = int.Parse(db.GetAttribute("card_season"));
                                string pos = db.GetAttribute("card_pos");
                                if (pos == "10")
                                {
                                    int throws = int.Parse(db.GetAttribute("card_throws"));
                                    int bats = int.Parse(db.GetAttribute("card_bats"));
                                    int pp = int.Parse(db.GetAttribute("card_pp"));
                                    string arsenal = db.GetAttribute("card_pitch");
                                    Pitcher card = new Pitcher(name, team, id3, season, pos, throws, pp, arsenal);
                                    c1.Push(card);
                                }
                                else
                                {
                                    int throws = int.Parse(db.GetAttribute("card_throws"));
                                    int bats = int.Parse(db.GetAttribute("card_bats"));
                                    int bp = int.Parse(db.GetAttribute("card_bp"));
                                    int con = int.Parse(db.GetAttribute("card_con"));
                                    int def = int.Parse(db.GetAttribute("card_def"));
                                    Fielder card = new Fielder(name, team, id3, season, pos, bats, bp, con, def, "temp");
                                    d1.Enqueue(card);
                                }
                                break;
                            }
                        }
                    }
                    db.Close();
                }
            }
            r1.Close();
        }
        public static void load_Bench(this List<Card> d1)
        {
            XmlReader db;
            XmlReader r1;

            string fnp_db = "resource/card_player.xml";
            string fnp_p1 = "resource/p1_benchdeck.xml";

            try
            {
                XmlReaderSettings setting = new XmlReaderSettings();
                setting.IgnoreComments = true;
                setting.IgnoreWhitespace = true;

                db = XmlReader.Create(fnp_db, setting);
                r1 = XmlReader.Create(fnp_p1, setting);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            while (r1.Read())
            {
                if (r1.Name.CompareTo("Item") == 0 && r1.NodeType == XmlNodeType.Element)
                {
                    int id7 = int.Parse(r1.GetAttribute("card_id"));

                    while (db.Read())
                    {
                        if (db.Name.CompareTo("Item") == 0 && db.NodeType == XmlNodeType.Element)
                        {
                            int id6 = int.Parse(db.GetAttribute("card_id"));
                            if (id7 == id6)
                            {
                                string name = db.GetAttribute("card_name");
                                string team = db.GetAttribute("card_team");
                                int season = int.Parse(db.GetAttribute("card_season"));
                                string pos = db.GetAttribute("card_pos");
                                if (pos == "11")
                                {
                                    int throws = int.Parse(db.GetAttribute("card_throws"));
                                    int bats = int.Parse(db.GetAttribute("card_bats"));
                                    int pp = int.Parse(db.GetAttribute("card_pp"));
                                    string arsenal = db.GetAttribute("card_pitch");
                                    Pitcher card = new Pitcher(name, team, id6, season, pos, throws, pp, arsenal);
                                    d1.Add(card);
                                }
                                else
                                {
                                    int throws = int.Parse(db.GetAttribute("card_throws"));
                                    int bats = int.Parse(db.GetAttribute("card_bats"));
                                    int bp = int.Parse(db.GetAttribute("card_bp"));
                                    int con = int.Parse(db.GetAttribute("card_con"));
                                    int def = int.Parse(db.GetAttribute("card_def"));
                                    Fielder card = new Fielder(name, team, id6, season, pos, bats, bp, con, def, "temp");
                                    d1.Add(card);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            db.Close();
            r1.Close();
        }
        public static void load_Reserve(this Stack<Pitch> d1)
        {
            string fnp_db = "resource/card_pitch.xml";
            string fnp_p1 = "resource/p1_reservedeck.xml";

            XmlReader p1;
            XmlReader db;

            try
            {
                XmlReaderSettings setting = new XmlReaderSettings();
                setting.IgnoreComments = true;
                setting.IgnoreWhitespace = true;

                db = XmlReader.Create(fnp_db, setting);
                p1 = XmlReader.Create(fnp_p1, setting);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            while (p1.Read())
            {
                if (p1.Name.CompareTo("Item") == 0 && p1.NodeType == XmlNodeType.Element)
                {
                    int id1 = int.Parse(p1.GetAttribute("card_id"));
                    int num1 = int.Parse(p1.GetAttribute("num"));

                    while (db.Read())
                    {
                        if (db.Name.CompareTo("Item") == 0 && db.NodeType == XmlNodeType.Element)
                        {
                            int id = int.Parse(db.GetAttribute("card_id"));
                            if (id1 == id)
                            {
                                string name1 = db.GetAttribute("card_name");
                                string type = db.GetAttribute("card_type");
                                int cost = int.Parse(db.GetAttribute("card_cost"));
                                int move = int.Parse(db.GetAttribute("card_move"));
                                int effect = int.Parse(db.GetAttribute("card_effect"));

                                for (int i = 0; i < num1; i++)
                                {
                                    Pitch card = new Pitch(name1, id1, type, cost, move, effect);
                                    d1.Push(card);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            db.Close();
            p1.Close();
        }
        public static void Draw<Pitch>(this Stack<Pitch> deck, List<Pitch> hand)
        {
            if (hand.Count() < 5)
            {
                hand.Add(deck.Peek());
                deck.Pop();
            }
        }
        public static void showHand(PictureBox[] pic, List<Pitch> hand)
        {
            for (int i = 0; i < 5; i++)
            {
                if (pic[i].Image != null)
                {
                    pic[i].Image = null;
                }
            }


            int count = hand.Count();

            for (int i = 0; i < count; i++)
            {
                int file_id = hand.ElementAt(i).card_id;
                string file_string = file_id.ToString();
                string file_path = "resource/card_pitch/" + file_id + ".jpg";
                pic[i].Image = Image.FromFile(file_path);
            }
        }

        public static void load_Batter(Queue<Fielder> order, PictureBox a, PictureBox b)
        {
            if (order.First<Fielder>().card_bats == Constants.LEFT)
            {
                int file_id = order.First<Fielder>().card_id;
                string file_string = file_id.ToString();
                string file_path = "resource/card_batter/" + file_id + ".jpg";
                a.Image = Image.FromFile(file_path);
            }
            else if (order.First<Fielder>().card_bats == Constants.RIGHT)
            {
                int file_id = order.First<Fielder>().card_id;
                string file_string = file_id.ToString();
                string file_path = "resource/card_batter/" + file_id + ".jpg";
                b.Image = Image.FromFile(file_path);
            }
        }
        public static void load_Batter(Stack<Fielder> order, PictureBox a, PictureBox b)
        {
            if (order.First<Fielder>().card_bats == Constants.LEFT)
            {
                int file_id = order.First<Fielder>().card_id;
                string file_string = file_id.ToString();
                string file_path = "resource/card_batter/" + file_id + ".jpg";
                a.Image = Image.FromFile(file_path);
            }
            else if (order.First<Fielder>().card_bats == Constants.RIGHT)
            {
                int file_id = order.First<Fielder>().card_id;
                string file_string = file_id.ToString();
                string file_path = "resource/card_batter/" + file_id + ".jpg";
                b.Image = Image.FromFile(file_path);
            }
        }
        public static void load_Batter(int order, int bats, PictureBox a, PictureBox b)
        {
            if (bats == Constants.LEFT)
            {
                int file_id = order;
                string file_string = file_id.ToString();
                string file_path = "resource/card_batter/" + file_id + ".jpg";
                a.Image = Image.FromFile(file_path);
            }
            else if (bats == Constants.RIGHT)
            {
                int file_id = order;
                string file_string = file_id.ToString();
                string file_path = "resource/card_batter/" + file_id + ".jpg";
                b.Image = Image.FromFile(file_path);
            }
        }

        public static void load_Pitcher(Stack<Pitcher> current, PictureBox a)
        {
            int file_id = current.First<Pitcher>().card_id;
            string file_string = file_id.ToString();
            string file_path = "resource/card_pitcher/" + file_id + ".jpg";
            a.Image = Image.FromFile(file_path);

        }
        public static void load_Pitcher(int current, PictureBox a)
        {
            int file_id = current;
            string file_string = file_id.ToString();
            string file_path = "resource/card_pitcher/" + file_id + ".jpg";
            a.Image = Image.FromFile(file_path);
        }

        public static void load_Manager(PictureBox a)
        {
            int file_id = 20160188;
            string file_string = file_id.ToString();
            string file_path = "resource/" + file_id + ".jpg";
            a.Image = Image.FromFile(file_path);
        }

        public static void load_StrikeZone(PictureBox[] pic, ref int[,] dz)
        {
            Stack<int> temp = new Stack<int>();
            temp.Push(0);
            for (int i = 2; i < 10; i++)
            {
                temp.Push(i);
            }
            temp.Shuffle();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    dz[i, j] = temp.Pop();
                }
            }
            for (int j = 0; j < 9; j++)
            {
                string file_path = "resource/bears.jpg";
                pic[j].Image = Image.FromFile(file_path);
            }
        }
        public static void load_StrikeZone(PictureBox[] pic)
        {
            for (int j = 0; j < 9; j++)
            {
                string file_path = "resource/bears.jpg";
                pic[j].Image = Image.FromFile(file_path);
            }
        }

        public static void next_Batter(Queue<Fielder> d, PictureBox a, PictureBox b)
        {
            if (d.Peek().card_bats == Constants.LEFT)
            {
                a.Image = null;
            }
            else
            {
                b.Image = null;
            }
            Fielder temp;
            temp = d.First<Fielder>();
            d.Dequeue();
            d.Enqueue(temp);
        }

        public static void merge_Decks(Stack<Pitch> m, Stack<Pitch> d, List<Pitch> h)
        {
            for (int i = 0; i < h.Count(); i++)
            {
                m.Push(h[i]);
                h.RemoveAt(i);
            }
            for (int i = 0; i < d.Count(); i++)
            {
                m.Push(d.Peek());
                d.Pop();
            }
        }

        public static void load_Bases(string[] bases, PictureBox[] b)
        {
            for (int i = 0; i < 3; i++)
            {
                if (bases[i+1] != null)
                {
                    b[i].BackColor = Color.Firebrick;
                }
                else
                {
                    b[i].BackColor = Color.Green;
                }
            }
        }
        public static void load_Bases(bool[] bases, PictureBox[] b)
        {
            for (int i = 0; i < 3; i++)
            {
                if (bases[i + 1])
                {
                    b[i].BackColor = Color.Firebrick;
                }
                else
                {
                    b[i].BackColor = Color.Green;
                }
            }
        }

        public static void load_Fielders(Label[] l, Fielder[] f, Stack<Pitcher> p)
        {
            l[0].Text = p.Peek().card_name;
            for (int i = 2; i < 10; i++ )
            {
                l[i-1].Text = f[i].card_name;
            }
        }
        public static void load_Fielders(Label[] l, string[] f)
        {
            l[0].Text = f[0];
            for (int i = 2; i < 10; i++)
            {
                l[i - 1].Text = f[i];
            }
        }

        public static void load_Score(PictureBox[] rhb1, PictureBox[] rhb2, PictureBox[] osb, PictureBox[] away, PictureBox[] home,  
            Situation h)
        {
            osb[0].Image = Image.FromFile("resource/" + h.outs + "out" + ".png");
            osb[1].Image = Image.FromFile("resource/" + h.strike + "strike" + ".png");
            osb[2].Image = Image.FromFile("resource/" + h.ball + "ball" + ".png");
            if (h.inning)
            {                
                if (h.opp_runs <= 20)
                {
                    rhb1[0].Image = Image.FromFile("resource/" + "_" + h.opp_runs + ".png");
                }
                else
                {
                    rhb1[0].Image = Image.FromFile("resource/EXCEED.png");
                }

                if (h.opp_hits <= 20)
                {
                    rhb1[1].Image = Image.FromFile("resource/" + "_" + h.opp_hits + ".png");
                }
                else
                {
                    rhb1[1].Image = Image.FromFile("resource/EXCEED.png");
                }
                rhb1[2].Image = Image.FromFile("resource/" + "_" + h.opp_walks + ".png");

                if (h.inning_runs <= 20)
                {
                    away[h.inning_count - 1].Image = Image.FromFile("resource/" + "_" + h.inning_runs + ".png");
                }
                else
                {
                    away[h.inning_count - 1].Image = Image.FromFile("resource/EXCEED.png");
                }
            }
            else
            {
                if (h.runs <= 20)
                {
                    rhb2[0].Image = Image.FromFile("resource/" + "_" + h.runs + ".png");
                }
                else
                {
                    rhb2[0].Image = Image.FromFile("resource/EXCEED.png");
                }

                if (h.hits <= 20)
                {
                    rhb2[1].Image = Image.FromFile("resource/" + "_" + h.hits + ".png");
                }
                else
                {
                    rhb2[1].Image = Image.FromFile("resource/EXCEED.png");
                }
                rhb2[2].Image = Image.FromFile("resource/" + "_" + h.walks + ".png");

                if (h.inning_runs <= 20)
                {
                    home[h.inning_count - 1].Image = Image.FromFile("resource/" + "_" + h.inning_runs + ".png");
                }
                else
                {
                    home[h.inning_count - 1].Image = Image.FromFile("resource/EXCEED.png");
                }
            }
        }

        public static void init_zone(bool[,] z)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    z[i, j] = false;
                }
            }
        }
        public static void init_zone(int[,] z)
        {
            Stack<int> deck = new Stack<int>();
            deck.Push(0);
            for (int i = 2; i < 10; i++)
            {
                deck.Push(i);
            }
            Shuffle<int>(deck);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    z[i, j] = deck.Pop();
                }
            }
        }
        public static void init_zone2(bool[,] z)
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    z[i, j] = false;
                    if (((i > 1) && (i < 5)) && ((j > 1) && (j < 5)))
                    {
                        z[i, j] = true;
                    }
                }
            }
        }
        public static void copy_zone_off(bool[,] z, Situation h)
        {
            z[0, 0] = h.zone0[0]; z[0, 1] = h.zone0[1]; z[0, 2] = h.zone0[2];
            z[1, 0] = h.zone1[0]; z[1, 1] = h.zone1[1]; z[1, 2] = h.zone1[2];
            z[2, 0] = h.zone2[0]; z[2, 1] = h.zone2[1]; z[2, 2] = h.zone2[2];
        }
        public static void copy_zone_def(bool[,] z, Situation h)
        {
            h.zone0[0] = z[0, 0]; h.zone0[1] = z[0, 1]; h.zone0[2] = z[0, 2];
            h.zone1[0] = z[1, 0]; h.zone1[1] = z[1, 1]; h.zone1[2] = z[1, 2];
            h.zone2[0] = z[2, 0]; h.zone2[1] = z[2, 1]; h.zone2[2] = z[2, 2];

        }

        public static void comp_disable(PictureBox[] b)
        {
            for (int i = 0; i < b.Count(); i++)
            {
                b[i].Enabled = false;
            }
        }
        public static void comp_enable(PictureBox[] b)
        {
            for (int i = 0; i < b.Count(); i++)
            {
                b[i].Enabled = true;
            }
        }
        public static void comp_disable(Button[] b)
        {
            for (int i = 0; i < b.Count(); i++)
            {
                b[i].Enabled = false;
            }
        }
        public static void comp_enable(Button[] b)
        {
            for (int i = 0; i < b.Count(); i++)
            {
                b[i].Enabled = true;
            }
        }

        public static void show_loc(bool[,] z, PictureBox[] b)
        {
            int k = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (z[i,j])
                    {
                        b[k].Image = null;
                        b[k].BackColor = Color.Black;
                    }
                    k++;
                }
            }
        }

        public static void pitch_move(bool[,] z, Pitch p, Stack<Pitcher> c, ref int x, ref int y)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (z[i, j])
                    {
                        switch (p.card_move)
                        {
                            case 0:
                                x = i + 2;
                                y = j + 2;
                                break;
                            case 1:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 2;
                                    y = j + 1;
                                }
                                else
                                {
                                    x = i + 2;
                                    y = j + 3;
                                }
                                break;
                            case 2:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 2;
                                    y = j + 3;
                                }
                                else
                                {
                                    x = i + 2;
                                    y = j + 1;
                                }
                                break;
                            case 3:
                                x = i + 1;
                                y = j + 2;
                                break;
                            case 4:
                                x = i + 3;
                                y = j + 2;
                                break;
                            case 5:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 3;
                                    y = j + 1;
                                }
                                else
                                {
                                    x = i + 3;
                                    y = j + 3;
                                }
                                break;
                            case 6:
                                if (c.Peek().card_throws == Constants.LEFT)
                                {
                                    x = i + 3;
                                    y = j + 1;
                                }
                                else
                                {
                                    x = i + 3;
                                    y = j + 3;
                                }
                                break;
                            case 7:
                                x = i + 4;
                                y = j + 2;
                                break;
                            case 8:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 3;
                                    y = j;
                                }
                                else
                                {
                                    x = i + 3;
                                    y = j + 4;
                                }
                                break;
                            case 9:
                                if (c.Peek().card_throws == Constants.LEFT)
                                {
                                    x = i + 3;
                                    y = j;
                                }
                                else
                                {
                                    x = i + 3;
                                    y = j + 4;
                                }
                                break;
                            case 10:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 4;
                                    y = j + 1;
                                }
                                else
                                {
                                    x = i + 4;
                                    y = j + 3;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
        public static void pitch_move(bool[,] z, Pitch p, Stack<Pitcher> c, ref int x, ref int y, ref int a, ref int b)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (z[i, j])
                    {
                        switch (p.card_move)
                        {
                            case 0:
                                a = i; b = j; x = i + 2; y = j + 2;
                                break;
                            case 1:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 2; a = i;
                                    y = j + 1; b = j - 1;
                                }
                                else
                                {
                                    x = i + 2; a = i;
                                    y = j + 3; b = j + 1;
                                }
                                break;
                            case 2:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 2; a = i;
                                    y = j + 1; b = j - 1;
                                }
                                else
                                {
                                    x = i + 2; a = i;
                                    y = j + 3; b = j + 1;
                                }
                                break;
                            case 3:
                                x = i + 1; a = i - 1;
                                y = j + 2; b = j;
                                break;
                            case 4:
                                x = i + 3; a = i + 1;
                                y = j + 2; b = j;
                                break;
                            case 5:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 3; a = i + 1;
                                    y = j + 1; b = j - 1;
                                }
                                else
                                {
                                    x = i + 3; a = i + 1;
                                    y = j + 3; b = j + 1;
                                }
                                break;
                            case 6:
                                if (c.Peek().card_throws == Constants.LEFT)
                                {
                                    x = i + 3; a = i + 1;
                                    y = j + 1; b = j - 1;
                                }
                                else
                                {
                                    x = i + 3; a = i + 1;
                                    y = j + 3; b = j + 1;
                                }
                                break;
                            case 7:
                                x = i + 4; a = i + 1;
                                y = j + 2; b = j;
                                break;
                            case 8:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 3; a = i + 1;
                                    y = j; b = j - 2;
                                }
                                else
                                {
                                    x = i + 3; a = i + 1;
                                    y = j + 4; b = j + 2;
                                }
                                break;
                            case 9:
                                if (c.Peek().card_throws == Constants.LEFT)
                                {
                                    x = i + 3; a = i + 1;
                                    y = j; b = j - 2;
                                }
                                else
                                {
                                    x = i + 3; a = i + 1;
                                    y = j + 4; b = j - 2;
                                }
                                break;
                            case 10:
                                if (c.Peek().card_throws == Constants.RIGHT)
                                {
                                    x = i + 4; a = i + 2;
                                    y = j + 1; b = j - 1;
                                }
                                else
                                {
                                    x = i + 4; a = i + 2;
                                    y = j + 3; b = j + 1;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public static void show_fielder(Fielder[] d, PictureBox[] i, int[,] z, int a, int b)
        {
            int file_id = d[z[a, b]].card_id;
            string file_string = file_id.ToString();
            string file_path = "resource/card_fielder/" + file_id + ".jpg";
            if (a == 0)
            {
                if (b == 0)
                {
                    i[0].Image = Image.FromFile(file_path);
                }
                else if (b == 1)
                {
                    i[1].Image = Image.FromFile(file_path);
                }
                else
                {
                    i[2].Image = Image.FromFile(file_path);
                }
            }
            else if (a == 1)
            {
                if (b == 0)
                {
                    i[3].Image = Image.FromFile(file_path);
                }
                else if (b == 1)
                {
                    i[4].Image = Image.FromFile(file_path);
                }
                else
                {
                    i[5].Image = Image.FromFile(file_path);
                }
            }
            else
            {
                if (b == 0)
                {
                    i[6].Image = Image.FromFile(file_path);
                }
                else if (b == 1)
                {
                    i[7].Image = Image.FromFile(file_path);
                }
                else
                {
                    i[8].Image = Image.FromFile(file_path);
                }
            }
        }

        public static void send(Situation h, Socket s)
        {
            JavaScriptSerializer json = new JavaScriptSerializer();
            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            var a = enc.GetByteCount(json.Serialize(h));
            byte[] msg = new byte[a];
            msg = enc.GetBytes(json.Serialize(h));

            s.Send(msg);
        }
    }
}
