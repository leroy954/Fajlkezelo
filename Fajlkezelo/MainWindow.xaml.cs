using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;

namespace Fajlkezelo
{
    public partial class MainWindow : Window
    {
        string aktualis = "";

        public MainWindow()
        {
            InitializeComponent();

            aktualis = Directory.GetCurrentDirectory();
            fajllista.MouseDoubleClick += Fajllista_MouseDoubleClick;
            betolt();
        }

        private void Fajllista_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string valasztott = fajllista.SelectedItem as string;
            if (valasztott == null)
                return;

            if (valasztott == "..")
            {
                DirectoryInfo szulo = Directory.GetParent(aktualis);

                if (szulo != null)
                    BetoltUtvonal(szulo.FullName);

                return;
            }

            if (valasztott.StartsWith("📁"))
            {
                string mappanev = valasztott.Substring("📁".Length);
                BetoltUtvonal(Path.Combine(aktualis, mappanev));
            }
            else if (valasztott.StartsWith("📄"))
            {
                string fajlnev = valasztott.Substring("📄".Length);
                string fajlUtvonal = Path.Combine(aktualis, fajlnev);

                try
                {
                    string tartalom = File.ReadAllText(fajlUtvonal);

                    szoveg.Document.Blocks.Clear();
                    szoveg.Document.Blocks.Add(
                        new Paragraph(new Run(tartalom))
                    );

                    FileInfo info = new FileInfo(fajlUtvonal);

                    fajlInformacio.Text =
                        $"Méret: {info.Length:N0} bájt  " +
                        $"({info.Length / 1024.0:N2} KiB)" +
                        $"    |    Létrehozva: {info.CreationTime:yyyy.MM.dd. HH:mm}" +
                        $"    |    Módosítva: {info.LastWriteTime:yyyy.MM.dd. HH:mm}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "A fájl nem olvasható szövegként:\n" + ex.Message
                    );
                }
            }
        }

        public void betolt()
        {
            BetoltUtvonal(aktualis);
        }

        private void BetoltUtvonal(string utvonal)
        {
            try
            {
                string[] konyvtarak = Directory.GetDirectories(utvonal);
                string[] fajlok = Directory.GetFiles(utvonal);

                // Csak sikeres beolvasás után váltunk könyvtárat.
                aktualis = utvonal;
                szoveg.Document.Blocks.Clear();
                fajlInformacio.Text = "Nincs megnyitott fájl";
                currentLabel.Content = aktualis;
                fajllista.Items.Clear();

                if (Directory.GetParent(aktualis) != null)
                    fajllista.Items.Add("..");

                foreach (string konyvtar in konyvtarak)
                    fajllista.Items.Add("📁" + Path.GetFileName(konyvtar));

                foreach (string fajl in fajlok)
                    fajllista.Items.Add("📄" + Path.GetFileName(fajl));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "A mappa nem nyitható meg:\n" + ex.Message
                );
            }
        }

        public void ListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // A megnyitást most a dupla kattintás kezeli.
        }

        public void athelyezes()
        {
            string sourceDirectory = @"C:\source";
            string destinationDirectory = @"C:\destination";

            try
            {
                Directory.Move(sourceDirectory, destinationDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void NewFolder_Click(object sender, RoutedEventArgs e)
        {
            Window ablak = new Window
            {
                Title = "Új mappa",
                Width = 320,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel tartalom = new StackPanel
            {
                Margin = new Thickness(15)
            };

            tartalom.Children.Add(new TextBlock
            {
                Text = "Mappa neve:"
            });

            TextBox nevMezo = new TextBox
            {
                Margin = new Thickness(0, 8, 0, 12)
            };
            tartalom.Children.Add(nevMezo);

            StackPanel gombok = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button megse = new Button
            {
                Content = "Mégse",
                Width = 70,
                Margin = new Thickness(0, 0, 8, 0)
            };
            megse.Click += (s, args) => ablak.Close();

            Button ok = new Button
            {
                Content = "OK",
                Width = 70,
                IsDefault = true
            };

            ok.Click += (s, args) =>
            {
                string nev = nevMezo.Text.Trim();

                if (string.IsNullOrWhiteSpace(nev) ||
                    nev == "." ||
                    nev == ".." ||
                    nev.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    MessageBox.Show("Adj meg egy érvényes mappanevet.");
                    return;
                }

                string ujMappa = Path.Combine(aktualis, nev);

                if (Directory.Exists(ujMappa) || File.Exists(ujMappa))
                {
                    MessageBox.Show("Már létezik ilyen nevű fájl vagy mappa.");
                    return;
                }

                try
                {
                    Directory.CreateDirectory(ujMappa);
                    ablak.Close();
                    betolt(); // Frissíti a jelenlegi mappa listáját.
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Nem sikerült létrehozni a mappát:\n" + ex.Message);
                }
            };

            gombok.Children.Add(megse);
            gombok.Children.Add(ok);
            tartalom.Children.Add(gombok);

            ablak.Content = tartalom;
            ablak.Loaded += (s, args) => nevMezo.Focus();
            ablak.ShowDialog();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void szoveg_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}