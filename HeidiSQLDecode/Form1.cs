using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeidiSQLDecode {
    public partial class Form1 : Form {

        private bool registry_loaded = false;

        public Form1() {
            InitializeComponent();
        }

        
        private void button1_Click(object sender, EventArgs e) {
            string[] lines = input.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            if (lines.Length > 0) {
                decoded.Clear();

                foreach (String line in lines) {
                    decoded.AppendText(decode(line) + System.Environment.NewLine);
                }
            }
           

        }

        private String decode(String input) {
            String err = "*COULD NOT DECODE*";
            var str = "";
            String hex = input.Trim();
            if (String.IsNullOrEmpty(hex)) {
                return err;
            }
            int shift;
            if(!int.TryParse(hex.Substring(hex.Length - 1, 1), out shift)) {
                return err;
            }
            hex = hex.Substring(0, hex.Length - 1);
            for (var i = 0; i < hex.Length; i += 2) {
                int tempval;
                try {
                    if (!int.TryParse(hex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.CurrentCulture, out tempval)) {
                        return err;
                    }
                } catch (ArgumentOutOfRangeException) {
                    return err;
                }
                str += (char)(tempval - shift);
            }
            return (String.IsNullOrEmpty(str) ? "*EMPTY*" : str);
        }

        private void button2_Click(object sender, EventArgs e) {
            input.Clear();
            decoded.Clear();
        }

        private void button3_Click(object sender, EventArgs e) {

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "HeidiSQL exported file (*.txt)|*.txt";
            dlg.CheckFileExists = true;
            dlg.ValidateNames = true;
            dlg.Multiselect = false;
            dlg.Title = "Please select exported file";

            if(dlg.ShowDialog() == DialogResult.OK) {

                decoded_file.Clear();

                
                using (StreamReader sr = new StreamReader(dlg.FileName)) {
                    String line = sr.ReadToEnd();
                    

                    try {
                        Regex regexObj = new Regex(@"(?m:^)Servers\\(.+)\\Password(.+)<\|\|\|>([A-Z0-9]+)");

                        foreach (Match ItemMatch in regexObj.Matches(line)) {
                            decoded_file.AppendText(ItemMatch.Groups[1] + " - " + ItemMatch.Groups[3] + " - " + decode(ItemMatch.Groups[3].Value) + System.Environment.NewLine);
                        }

                    } catch (ArgumentException) {
                        // Syntax error in the regular expression
                    }

      
                }


            }
        }

        private void button4_Click(object sender, EventArgs e) {
            decoded_file.Clear();
        }

        private void load_registry() {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\HeidiSQL\\Servers");

            GetSubKeys(registryKey);

        }

        private void GetSubKeys(RegistryKey SubKey) {
            foreach (string sub in SubKey.GetSubKeyNames()) {
                
                RegistryKey local = Registry.CurrentUser.OpenSubKey("Software\\HeidiSQL\\Servers");
                local = SubKey.OpenSubKey(sub, true);
                if (local.GetValueNames().Contains("Password")) {
                    decoded_registry.AppendText(sub + " - " + local.GetValue("Password") + " - " + decode(local.GetValue("Password").ToString()) + System.Environment.NewLine);
                }
                GetSubKeys(local);
            }
        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e) {
            if(!registry_loaded && tabControl1.TabIndex == 2) {
                load_registry();
                registry_loaded = true;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            if (!registry_loaded && tabControl1.SelectedIndex == 2) {
                load_registry();
                registry_loaded = true;
            }
        }

        private void button5_Click(object sender, EventArgs e) {
            decoded_registry.Clear();
            load_registry();
        }
    }
}
