using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    
    public partial class OtomasyonForm : Form
    {
        
        OleDbConnection baglanti = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\\veritabani.mdb");
        OleDbDataReader okuyucu;
        OleDbCommand sorgu = new OleDbCommand();

        public OtomasyonForm()
        {
            InitializeComponent();
        }

        void veriCek()
        {
          
            if(baglanti.State== ConnectionState.Closed) baglanti.Open();

            OleDbDataAdapter Adapter = new OleDbDataAdapter("",baglanti);
            DataSet DataS = new DataSet();
            sorgu.Connection = baglanti;


            Adapter.SelectCommand.CommandText = "Select ID, plaka as [Plaka],marka as [Marka] ,model as [Model],renk as [Renk],yili as [Yılı],km as [Kilometre],gunlukFiyat as [Günlük Fiyat] from arac where musaitMi  = 1";
            Adapter.Fill(DataS, "musaitAraclar");
            Adapter.SelectCommand.CommandText = "Select * from musteri";
            Adapter.Fill(DataS, "musteriler");
            Adapter.SelectCommand.CommandText = "Select ID, plaka as [Plaka],marka as [Marka] ,model as [Model],renk as [Renk],yili as [Yılı],km as [Kilometre],gunlukFiyat as [Günlük Fiyat], switch( musaitMi = 0 , 'Hayır' , musaitMi = 1 , 'Evet'  ) as [Müsat Mi?] from arac";
            Adapter.Fill(DataS, "araclar");
            Adapter.SelectCommand.CommandText = "Select i.ID,m.ad,m.soyad,a.plaka,m.tcKimlik,i.tutar,i.alimTarihi,i.teslimTarihi from ((islem i inner join musteri m on m.ID = i.musteriNo ) inner join arac a on a.ID = i.aracNo)  where durum = 0 ";
            Adapter.Fill(DataS, "aktifislemler");
            Adapter.SelectCommand.CommandText = "select * from gecmisislem";
            Adapter.Fill(DataS, "gecmisislem");



            DgwKiralamaAraclar.DataSource = DataS.Tables["musaitAraclar"];
            DgwKiralamaMusteriler.DataSource = DataS.Tables["musteriler"];
            DgwAraclar.DataSource = DataS.Tables["araclar"];
            DgwMusteriler.DataSource = DataS.Tables["musteriler"];
            DgwAktifislemler.DataSource = DataS.Tables["aktifislemler"];
            DgwGecmisislemler.DataSource = DataS.Tables["gecmisislem"];


            DgwKiralamaAraclar.Columns[0].Visible = false;
            DgwKiralamaMusteriler.Columns[0].Visible = false;
            DgwAraclar.Columns[0].Visible = false;
            DgwMusteriler.Columns[0].Visible = false;
            DgwAktifislemler.Columns[0].Visible = false;
            DgwGecmisislemler.Columns[0].Visible = false;

            
            
           
        }


        private void OtomasyonForm_Load(object sender, EventArgs e)
        {
            veriCek();
        }





        // Araçlar Sekmesi 

        private void buttonAracEkle_Click(object sender, EventArgs e)
        {
            string plaka, marka, model, renk;
            int gunlukfiyat, km, yil;

            if ( string.IsNullOrEmpty(tbEkleGunlukFiyat.Text) 
              || string.IsNullOrEmpty(tbEkleKm.Text) 
              || string.IsNullOrEmpty(tbEkleMarka.Text)
              || string.IsNullOrEmpty(tbEkleModel.Text)
              || string.IsNullOrEmpty(tbEklePlaka.Text)  
              || string.IsNullOrEmpty(tbEkleRenk.Text)
              || string.IsNullOrEmpty(tbEkleYili.Text)) 
            {
                MessageBox.Show("Boş Alan Bırakmayınız");
            }
            else
            {
                plaka = tbEklePlaka.Text;
                marka = tbEkleMarka.Text;
                model = tbEkleModel.Text;
                renk = tbEkleRenk.Text;
                yil = int.Parse(tbEkleYili.Text);
                gunlukfiyat = int.Parse(tbEkleGunlukFiyat.Text);
                km = int.Parse(tbEkleKm.Text);

                sorgu.CommandText = "select id from arac where plaka  = '" + plaka + "'";
                

                okuyucu = sorgu.ExecuteReader();

                if (okuyucu.Read())
                {
                    MessageBox.Show("Bu plakaya ait bir araç bulunmakta");
                    okuyucu.Close();
                }
                else
                {
                    okuyucu.Close();
                    sorgu.CommandText = "insert into arac (plaka,marka,model,renk,yili,km,gunlukFiyat) values( '" + plaka + "','" + marka + "','" + model + "','" + renk + "'," + yil + "," + km + "," + gunlukfiyat + ")";

                    sorgu.ExecuteNonQuery();

                    veriCek();
                    MessageBox.Show("Araç Eklendi.");
                }
 
                

            }


        }

        private void ButtonAraciSil_Click(object sender, EventArgs e)
        {
            int aracno;
            string musaitMi;

            aracno = int.Parse(DgwAraclar.CurrentRow.Cells[0].Value.ToString());
            musaitMi = DgwAraclar.CurrentRow.Cells[8].Value.ToString();

            if (musaitMi == "Evet")
                {
                    sorgu.CommandText = "Delete from arac where ID = " + aracno + " and musaitMi = 1";
                    MessageBox.Show("Araç Silindi");
                    sorgu.ExecuteNonQuery();
                }
            else
                MessageBox.Show("Araba şu an müsait değil.Kiralama işlemi tamamlandıktan sonra silebilirsiniz.");

            

            veriCek();

            
        }

        private void ButtonPlakayaGoreSil_Click(object sender, EventArgs e)
        {
           

            
            
            string plaka,musaitMi;

            plaka = tbSilPlaka.Text;

            sorgu.CommandText = "select musaitMi from arac where plaka = '" + plaka + "'";

            okuyucu = sorgu.ExecuteReader();
            if (okuyucu.Read())
            {
                  musaitMi = okuyucu["musaitMi"].ToString();
                  
                  if (musaitMi == "1")
                  {
                      sorgu.CommandText = "Delete from arac where plaka = '" + plaka + "'";
                      MessageBox.Show("Araç Silindi");
                      okuyucu.Close();
                      sorgu.ExecuteNonQuery();
                  }
                  else
                     { MessageBox.Show("Araba şu an müsait değil.Kiralama işlemi tamamlandıktan sonra silebilirsiniz.");
                        okuyucu.Close(); 
                  }
            } 
            else
            {
                MessageBox.Show("Plaka Bulunamadı");
                okuyucu.Close();
            }

            
            

            
            veriCek();
            
            
          
        }

       

        int Garacno;
        bool Asecim = false;
        private void DgwAraclar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Asecim = true;
                Garacno = int.Parse(DgwAraclar.CurrentRow.Cells[0].Value.ToString());



                tbGuncellePlaka.Text = DgwAraclar.CurrentRow.Cells[1].Value.ToString();
                tbGuncelleMarka.Text = DgwAraclar.CurrentRow.Cells[2].Value.ToString();
                tbGuncelleModel.Text = DgwAraclar.CurrentRow.Cells[3].Value.ToString();
                tbGuncelleRenk.Text = DgwAraclar.CurrentRow.Cells[4].Value.ToString();
                tbGuncelleYili.Text = DgwAraclar.CurrentRow.Cells[5].Value.ToString();
                tbGuncelleKm.Text = DgwAraclar.CurrentRow.Cells[6].Value.ToString();
                tbGuncelleGunlukFiyat.Text = DgwAraclar.CurrentRow.Cells[7].Value.ToString();
            }
            
            
            catch (Exception)
            {
               
            }
            

        }


        private void buttonAracGuncelle_Click(object sender, EventArgs e)
        {

            string plaka, marka, model, renk;
            int gunlukfiyat, km, yil;

            if ( Asecim == false )
            {
                MessageBox.Show("Önce bir seçim yapmalısınız.");
            }
            else
            {
                if (string.IsNullOrEmpty(tbGuncelleGunlukFiyat.Text)
              || string.IsNullOrEmpty(tbGuncelleKm.Text)
              || string.IsNullOrEmpty(tbGuncelleMarka.Text)
              || string.IsNullOrEmpty(tbGuncelleModel.Text)
              || string.IsNullOrEmpty(tbGuncellePlaka.Text)
              || string.IsNullOrEmpty(tbGuncelleRenk.Text)
              || string.IsNullOrEmpty(tbGuncelleYili.Text))
                {
                    MessageBox.Show("Boş Alan Bırakmayın");
                
                    
                }
                else
                {
                    plaka = tbGuncellePlaka.Text;
                    marka = tbGuncelleMarka.Text;
                    model = tbGuncelleModel.Text;
                    renk = tbGuncelleRenk.Text;
                    yil = int.Parse(tbGuncelleYili.Text);
                    gunlukfiyat = int.Parse(tbGuncelleGunlukFiyat.Text);
                    km = int.Parse(tbGuncelleKm.Text);

                    sorgu.CommandText = "update arac set plaka = '" + plaka + "',marka = '" + marka + "',model = '" + model + "', renk = '" + renk + "',yili = " + yil + ",km = " + km + ",gunlukFiyat = " + gunlukfiyat + " where ID =" + Garacno + " ";

                    sorgu.ExecuteNonQuery();


                    veriCek();
                    MessageBox.Show("Araç Bilgileri Güncellendi");

                    Asecim = false;

                    tbGuncellePlaka.Text = "";
                    tbGuncelleMarka.Text = "";
                    tbGuncelleModel.Text = "";
                    tbGuncelleRenk.Text = "";
                    tbGuncelleYili.Text = "";
                    tbGuncelleKm.Text = "";
                    tbGuncelleGunlukFiyat.Text = "";

                }
            }

        }



        //Müşteriler Sekmesi
        private void buttonMusteriEkle_Click(object sender, EventArgs e)
        {
            string ad,soyad,adres,telefon;
            long tckimlik=0;

            if ( string.IsNullOrEmpty(tbEkleTcKimlik.Text) 
              || string.IsNullOrEmpty(tbEkleAd.Text) 
              || string.IsNullOrEmpty(tbEkleSoyad.Text)
              || string.IsNullOrEmpty(tbEkleAdres.Text)
              || string.IsNullOrEmpty(tbEkleTelefon.Text)  
               ) 
            {
                MessageBox.Show("Boş Alan Bırakmayınız");
            }
            else
            {

                try
                {
                    tckimlik = long.Parse(tbEkleTcKimlik.Text);
                }
                catch (Exception)
                {

                    MessageBox.Show("Lütfen Kimlik numarasını sayısal ifade kullanarak yazın.");
                    return;
                }
                    

                
                ad = tbEkleAd.Text;
                soyad = tbEkleSoyad.Text;
                adres = tbEkleAdres.Text;
                telefon = tbEkleTelefon.Text;

                sorgu.CommandText = "select id from musteri where tcKimlik  = "+tckimlik+"";
                

                okuyucu = sorgu.ExecuteReader();

                if (okuyucu.Read())
                {
                    MessageBox.Show("Bu TC kimlik numarasına kullanan bir müşteri bulunmaktadır.");
                    okuyucu.Close();
                }
                else
                {
                    okuyucu.Close();
                    sorgu.CommandText = "insert into musteri (tcKimlik,ad,soyad,adres,telefon) values( " + tckimlik + ",'" + ad + "','" + soyad + "','" + adres + "','" + telefon + "')";

                    sorgu.ExecuteNonQuery();

                    veriCek();
                    MessageBox.Show("Müşteri Eklendi.");
                    
                }

            }
        }

        private void buttonMusteriSil_Click(object sender, EventArgs e)
        {
            string musterino;


            musterino = DgwMusteriler.CurrentRow.Cells[0].Value.ToString();

            sorgu.CommandText = "select ID from islem where musteriNo = " + musterino + "";

            okuyucu = sorgu.ExecuteReader();

            if (okuyucu.Read())
            {
                MessageBox.Show("Müşterinin işlemler sekmesinde kaydı bulunmakta o kayıtlar silinmeden müşteriyi silemezsiniz.");
                okuyucu.Close();
            }
            else
            {

                okuyucu.Close();
                sorgu.CommandText = "Delete from musteri where ID = " + musterino + "";
                MessageBox.Show("Müşteri Silindi");
                sorgu.ExecuteNonQuery();
            }


            veriCek();
        }


        bool Msecim = false;
        int Gmusterino;
        private void DgwMusteriler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Msecim = true;

                Gmusterino = int.Parse(DgwMusteriler.CurrentRow.Cells[0].Value.ToString());



                tbGuncelleTcKimlik.Text = DgwMusteriler.CurrentRow.Cells[1].Value.ToString();
                tbGuncelleAd.Text = DgwMusteriler.CurrentRow.Cells[2].Value.ToString();
                tbGuncelleSoyad.Text = DgwMusteriler.CurrentRow.Cells[3].Value.ToString();
                tbGuncelleAdres.Text = DgwMusteriler.CurrentRow.Cells[4].Value.ToString();
                tbGuncelleTelefon.Text = DgwMusteriler.CurrentRow.Cells[5].Value.ToString();
            }
           
            
             catch (Exception)
            {
                
                
            }

           
        }

        private void buttonGuncelleMusteri_Click(object sender, EventArgs e)
        {
            string  ad, soyad, adres, telefon;
            long tckimlik = 0;
            
            if (Msecim == false)
            {
                MessageBox.Show("Önce bir seçim yapmalısınız.");
            }
            else

            {
                if ( string.IsNullOrEmpty(tbGuncelleTcKimlik.Text) 
              || string.IsNullOrEmpty(tbGuncelleAd.Text) 
              || string.IsNullOrEmpty(tbGuncelleSoyad.Text)
              || string.IsNullOrEmpty(tbGuncelleAdres.Text)
              || string.IsNullOrEmpty(tbGuncelleTelefon.Text))
                {
                    MessageBox.Show("Boş Alan Bırakmayın");


                }
                else
                {
                    try
                    {
                        tckimlik = long.Parse(tbGuncelleTcKimlik.Text);
                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Lütfen Kimlik numarasını sayısal ifade kullanarak yazın.");
                        return;
                    }
                    

                    ad = tbGuncelleAd.Text;
                    soyad = tbGuncelleSoyad.Text;
                    adres = tbGuncelleAdres.Text;
                    telefon = tbGuncelleTelefon.Text;

                    sorgu.CommandText = "update musteri set ad = '" + ad + "',soyad = '" + soyad + "',tcKimlik = '" + tckimlik + "', adres = '" + adres + "',telefon = '" + telefon + "' where ID =" + Gmusterino + " ";

                    sorgu.ExecuteNonQuery();


                    veriCek();
                    MessageBox.Show("Müşteri Bilgileri Güncellendi");

                    Msecim = false;

                    tbGuncelleTcKimlik.Text ="";
                    tbGuncelleAd.Text = "";
                    tbGuncelleSoyad.Text = "";
                    tbGuncelleAdres.Text = "";
                    tbGuncelleTelefon.Text = "";

                }
            }
        }


        //Kiralama İşlemleri Sekmesi
        private void btIslemiOnayla_Click(object sender, EventArgs e)
        {

            DateTime alinacak = new DateTime();
            DateTime verilecek = new DateTime();
            int musteriNo = 0, aracNo = 0, tutar = 0;

            alinacak = DtpTeslimAlınacak.Value;
            verilecek = DtpTeslimEdilecek.Value;

            try
            {
                tutar = int.Parse(tbTutar.Text);
            }
            catch (Exception)
            {

                MessageBox.Show("Lütfen Tutar Alanına Geçerli Bir Değer Girin");
            }
            

            
            if (alinacak >= verilecek)
            {
                MessageBox.Show("Verilecek Tarih Alınan Tarihten Daha İleri Olmalıdır");
            }
            else
            {
                try
                {
                    musteriNo = int.Parse(DgwKiralamaMusteriler.CurrentRow.Cells[0].Value.ToString());
                    aracNo = int.Parse(DgwKiralamaAraclar.CurrentRow.Cells[0].Value.ToString());
                    
                }
                catch (Exception)
                {

                    MessageBox.Show("Lütfen Seçim Yapınız");
                    return;
                }
               



                sorgu.CommandText = "Insert into islem ( musteriNo,aracNo,tutar,alimTarihi,teslimTarihi ) values ( " + musteriNo + "," + aracNo + "," + tutar + ",'" + alinacak + "','" + verilecek + "' )";
                sorgu.ExecuteNonQuery();

                sorgu.CommandText = "update arac set musaitMi = 0  where ID = "+ aracNo+" ";
                sorgu.ExecuteNonQuery();
                MessageBox.Show("İşlem Tamamlandı");
                veriCek();
            }   


        }

        //Aktif işlemler sekmesi
        bool Aisecim= false;
        int islemno;
        private void DgwAktifislemler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Aisecim = true;

                islemno = int.Parse(DgwAktifislemler.CurrentRow.Cells[0].Value.ToString());



                tbAktifTutar.Text = DgwAktifislemler.CurrentRow.Cells[5].Value.ToString();
                DtpAiTeslimAlınacak.Value = Convert.ToDateTime(DgwAktifislemler.CurrentRow.Cells[6].Value);
                DtpAiTeslimEdilecek.Value = Convert.ToDateTime(DgwAktifislemler.CurrentRow.Cells[7].Value);

            }
            
            
            catch (Exception)
            {
                
            }
        }

        private void buttonAiGuncelle_Click(object sender, EventArgs e)
        {

            int tutar=0;
            DateTime alinacak = new DateTime();
            DateTime verilecek = new DateTime();
            

            alinacak = DtpAiTeslimAlınacak.Value;
            verilecek = DtpAiTeslimEdilecek.Value;

            try
            {
                tutar = int.Parse(tbAktifTutar.Text);
            }
            catch (Exception)
            {

                MessageBox.Show("Lütfen Tutar Alanına Geçerli Bir Değer Girin");
            }

            if (Aisecim == false)
            {
                MessageBox.Show("Önce bir seçim yapmalısınız.");
            }
            else if (alinacak >= verilecek)
            {
                MessageBox.Show("Verilecek Tarih Alınan Tarihten Daha İleri Olmalıdır");
            }
            else
            {
                if (string.IsNullOrEmpty(tbAktifTutar.Text))
                {
                    MessageBox.Show("Boş Alan Bırakmayın");


                }
                else
                {
                    
                    sorgu.CommandText = "update islem set tutar = " +tutar + ",alimTarihi = '" + alinacak + "',teslimTarihi = '" + verilecek + "' where ID =" + islemno + " ";

                    sorgu.ExecuteNonQuery();


                    veriCek();
                    MessageBox.Show("İşlem Bilgileri Güncellendi");

                    Aisecim = false;

                    tbAktifTutar.Text = "";
                    DtpAiTeslimAlınacak.Value = DateTime.Now;
                    DtpAiTeslimEdilecek.Value = DateTime.Now;
                }
            }
        }

        private void buttonAktifiptal_Click(object sender, EventArgs e)
        {
            if (Aisecim == false)
            {
                MessageBox.Show("Önce bir seçim yapmalısınız.");
            }
            else
            {
                sorgu.CommandText = "update arac set musaitMi=1 where ID = (select aracNo from islem where ID = " + islemno + ")";
                sorgu.ExecuteNonQuery();

                sorgu.CommandText = "Delete from islem where ID = " + islemno + "";
                sorgu.ExecuteNonQuery();
                MessageBox.Show("Seçilen işlem iptal edildi.");
                veriCek();
            }
        }

        private void buttonislemTamamlandı_Click(object sender, EventArgs e)
        {


            int tutar = 0,islemno;
            string ad = "", soyad = "", plaka = "", tcKimlik ="" ;

            DateTime alinacak = new DateTime();
            DateTime verilecek = new DateTime();

            if (Aisecim == false)
            {
                MessageBox.Show("Önce bir seçim yapmalısınız.");
            }
            else
            {
                
                islemno = int.Parse(DgwAktifislemler.CurrentRow.Cells[0].Value.ToString());
                ad = DgwAktifislemler.CurrentRow.Cells[1].Value.ToString();
                soyad = DgwAktifislemler.CurrentRow.Cells[2].Value.ToString();
                plaka = DgwAktifislemler.CurrentRow.Cells[3].Value.ToString();
                tcKimlik = DgwAktifislemler.CurrentRow.Cells[4].Value.ToString();
                tutar = int.Parse(DgwAktifislemler.CurrentRow.Cells[5].Value.ToString());

                


                alinacak = Convert.ToDateTime(DgwAktifislemler.CurrentRow.Cells[6].Value);
                verilecek = Convert.ToDateTime(DgwAktifislemler.CurrentRow.Cells[7].Value);

                sorgu.CommandText = "insert into gecmisislem (tcKimlik,ad,soyad,plaka,alimTarihi,teslimTarihi,tutar) Values ('"+tcKimlik+"','"+ad+"','"+soyad+"','"+plaka+"','"+alinacak+"','"+verilecek+"','"+tutar+"') ";
                
                sorgu.ExecuteNonQuery();

                sorgu.CommandText = "delete from islem where ID = "+islemno+" ";

                sorgu.ExecuteNonQuery();

                sorgu.CommandText = "update arac set musaitMi = 1 where plaka = '" + plaka + "' ";

                sorgu.ExecuteNonQuery();

                veriCek();
            }
        }

       // Kiralama Geçmişi Sekmesi


        bool Gislemsecim = false;
        int Gislemno;
        private void DgwGecmisislemler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {

                Gislemsecim = true;

                Gislemno = int.Parse(DgwGecmisislemler.CurrentRow.Cells[0].Value.ToString());
            }
            
            catch (Exception)
            {
                
                
            }

        }

        private void buttonKaydiSil_Click(object sender, EventArgs e)
        {
            if (Gislemsecim == false)
            {
                MessageBox.Show("Önce bir seçim yapmalısınız.");
            }
            else
            {
                sorgu.CommandText = "Delete from gecmisislem where ID = " + Gislemno + "";
                sorgu.ExecuteNonQuery();
                MessageBox.Show("Kayıt Silindi");
                veriCek();
                Gislemsecim = false;
            }

        }

        


    }
}
