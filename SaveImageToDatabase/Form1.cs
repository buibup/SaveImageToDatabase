using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaveImageToDatabase
{
    public partial class Form1 : Form
    {
        private string _fileName;
        private List<MyPicture> _list;

        public Form1(List<MyPicture> list)
        {
            this._list = list;
            InitializeComponent();
        }

        public Form1()
        {
            InitializeComponent();
        }

        Image ConvertBinaryToImag(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return Image.FromStream(ms);
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.FocusedItem != null)
            {
                pictureBox.Image = ConvertBinaryToImag(_list[listView.FocusedItem.Index].Data);
                lblFileName.Text = listView.FocusedItem.SubItems[0].Text;
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    Filter = @"JPEG|*.jpg",
                    ValidateNames = true,
                    Multiselect = false
                })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _fileName = ofd.FileName;
                    lblFileName.Text = _fileName;
                    pictureBox.Image = Image.FromFile(_fileName);
                }
            }
        }

        byte[] ConvertImageToBinary(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            using (PicEntities db = new PicEntities())
            {
                var pic = new MyPicture() { FileName = _fileName, Data = ConvertImageToBinary(pictureBox.Image) };
                db.MyPictures.Add(pic);
                try
                {
                    await db.SaveChangesAsync();
                    MessageBox.Show(@"You have been successfully saved.", @"Message", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (DbEntityValidationException dbEx)
                {

                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Console.WriteLine(@"Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }

            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            listView.Items.Clear();
            using (PicEntities db = new PicEntities())
            {
                _list = db.MyPictures.ToList();
                foreach (var pic in _list)
                {
                    ListViewItem item = new ListViewItem(pic.FileName);
                    listView.Items.Add(item);
                }
            }
        }
    }
}
