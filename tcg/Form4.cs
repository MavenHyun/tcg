using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace tcg
{
    public partial class Form4 : Form
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        List<int> pdeck = new List<int>(15);
        List<int> bdeck = new List<int>(15);
        int[] arsenal = new int[6];

        public Form4()
        {
            InitializeComponent();

            list_cards.MouseDoubleClick += new MouseEventHandler(list_cards_MouseDoubleClick);
            group_bdeck.MouseClick += new MouseEventHandler(group_bdeck_MouseClick);

            Support.load_db(dict);
            Support.load_deck("doosandeck", pdeck, bdeck, group_pdeck, group_bdeck, group_rdeck);  
            
        }

        public static class Support
        {
      


            public static void load_db(Dictionary<string, string> dict)
            {
                string fnp2 = "resource/card_player.xml";
                XmlReader db;

                try
                {
                    XmlReaderSettings setting = new XmlReaderSettings();
                    setting.IgnoreComments = true;
                    setting.IgnoreWhitespace = true;
                    db = XmlReader.Create(fnp2, setting);
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
                        string name = db.GetAttribute("card_name");
                        string id = db.GetAttribute("card_id");
                        dict.Add(id, name);
                    }
                }
                db.Close();
            }

            public static void load_deck(string deck_name, List<int> pdeck, List<int> bdeck, List<int> rdeck,
                                         GroupBox mgroup, GroupBox pgroup, GroupBox bgroup, GroupBox rgroup)
            {
                string deck = "resource/" + deck_name + ".xml";
                XmlReader dreader;

                try
                {
                    XmlReaderSettings setting = new XmlReaderSettings();
                    setting.IgnoreComments = true;
                    setting.IgnoreWhitespace = true;

                    dreader = XmlReader.Create(deck, setting);

                }
                catch (System.Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }

                while (dreader.Read())
                {
                    if (dreader.Name.CompareTo("PCard") == 0 && dreader.NodeType == XmlNodeType.Element)
                    {
                        pdeck.Add(int.Parse(dreader.GetAttribute("card_id")));
                    }
                    else if (dreader.Name.CompareTo("MCard") == 0 && dreader.NodeType == XmlNodeType.Element)
                    {
                        int num = int.Parse(dreader.GetAttribute("num"));
                        string hex = dreader.GetAttribute("card_id");
                        int index = Convert.ToInt32(hex, 16);
                        string dec = Convert.ToInt32(hex, 16).ToString();
                        for (int i = 0; i < num; i++)
                        {
                            mdeck.Add(int.Parse(dec));
                        }
                        foreach (Control c in mgroup.Controls)
                        {
                            if (c is Label)
                            {
                                if (mdeck.Last() == 14 - mgroup.Controls.IndexOf(c))
                                {
                                    Label l = new Label();
                                    l = (Label)c;
                                    int i = mgroup.Controls.IndexOf(c);
                                    l.Text = num.ToString();
                                }
                            }
                            else if (c is PictureBox)
                            {
                                if (mdeck.Last() == 29 - mgroup.Controls.IndexOf(c))
                                {
                                    PictureBox p = new PictureBox();
                                    p = (PictureBox)c;
                                    string file_path = "resource/card_pitch/" + hex + ".jpg";
                                    p.Image = Image.FromFile(file_path);
                                }   
                            }
                        }
                    }
                    else if (dreader.Name.CompareTo("BCard") == 0 && dreader.NodeType == XmlNodeType.Element)
                    {
                        bdeck.Add(int.Parse(dreader.GetAttribute("card_id")));
                    }
                    else if (dreader.Name.CompareTo("RCard") == 0 && dreader.NodeType == XmlNodeType.Element)
                    {
                        int num = int.Parse(dreader.GetAttribute("num"));
                        string hex = dreader.GetAttribute("card_id");
                        int index = Convert.ToInt32(hex, 16);
                        string dec = Convert.ToInt32(hex, 16).ToString();
                        for (int i = 0; i < num; i++)
                        {
                            rdeck.Add(int.Parse(dreader.GetAttribute("card_id")));
                        }
                        int temp = 0;
                        foreach (PictureBox p in rgroup.Controls)
                        {
                            int file_id = rdeck.ElementAt(temp);
                            string file_path = "resource/card_pitch/" + hex + ".jpg";
                            p.Image = Image.FromFile(file_path);
                            temp++;
                        }

                    }
                    else
                    {
                    }
                }
                dreader.Close();

                int count = 0;                
                foreach (PictureBox p in pgroup.Controls)
                {
                    int file_id = pdeck.ElementAt(9-count);
                    string file_path = "resource/card_images/" + file_id.ToString() + ".jpg";
                    p.Image = Image.FromFile(file_path);
                    count++;
                }

                count = 0;
                foreach (PictureBox p in bgroup.Controls)
                {
                    int file_id = bdeck.ElementAt(9-count);
                    string file_path = "resource/card_images/" + file_id.ToString() + ".jpg";
                    p.Image = Image.FromFile(file_path);
                    count++;
                }
            }
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            list_cards.Clear();
            image_cards.Dispose();
            int i = 0;
            foreach (var pair in dict)
            {
                if (pair.Value.Contains(text_search.Text))
                {
                    string fp = "resource/card_images/" + pair.Key + ".jpg";
                    image_cards.Images.Add(Image.FromFile(fp));
                    list_cards.LargeImageList = image_cards;
                    list_cards.Items.Add(new ListViewItem { ImageKey = pair.Key,  ImageIndex = i,  Text = pair.Value  });
                    i++;
                }
            }
        }

        private void btn_pitch_Click(object sender, EventArgs e)
        {
            list_cards.Clear();
            image_cards.Dispose();
            int i = 0;
            foreach (var pair in dict)
            {
                if (pair.Key.Length == 1)
                {
                    string fp = "resource/card_images/" + pair.Key + ".jpg";
                    image_cards.Images.Add(Image.FromFile(fp));
                    list_cards.LargeImageList = image_cards;
                    list_cards.Items.Add(new ListViewItem { ImageIndex = i, ImageKey = pair.Key, Text = pair.Value  });
                    i++;
                }
            }
        }


        private void list_cards_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = list_cards.HitTest(e.X, e.Y);
            ListViewItem item = info.Item;

            if ((item != null))
            {
                if (tab_control.SelectedTab == tab_mdeck)
                {
                    if (/*mdeck.Count == mdeck.Capacity*/item.ImageKey.Length == 8)
                    {
                        MessageBox.Show("덱은 꽉 차 있습니다... 카드를 먼저 빼세요!");
                    }
                    else if (item.Text.Length == 8)
                    {
                        MessageBox.Show("구종 카드가 아닙니다!");
                    }
                    else
                    {
                        
                    }
                }



            }
            else
            {
                this.list_cards.SelectedItems.Clear();
                MessageBox.Show("No Item is selected");
            }
        }

        private void group_bdeck_MouseClick(object sender, MouseEventArgs e)
        {

        }

      
    }
}
