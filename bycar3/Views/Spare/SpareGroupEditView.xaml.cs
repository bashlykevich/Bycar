using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using bycar;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for SpareGroupEditView.xaml
    /// </summary>
    public partial class SpareGroupEditView : Window
    {
        public string _parentName = null;
        public string _groupName = null;
     
        public SpareGroupEditView()
        {
            InitializeComponent();
        }
        public void LoadItem(string name)
        {
            _groupName = name;
            DataAccess da = new DataAccess();
            spare_group b = da.GetSpareGroup(name);            
            edtName.Text = b.name;
            edtDescr.Text = b.description;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            int res = 0;
            spare_group group = null;
            if (this._groupName != null)
                res = EditItem();
            else
                group = CreateItem();
            if (res == 0 && group == null)
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Группа с таким названием уже существует!");
            }
        }
        
        private int EditItem()
        {
            DataAccess da = new DataAccess();
            spare_group item = new spare_group();
            item.id = da.GetSpareGroups().FirstOrDefault(g => g.name == _groupName).id;
            item.name = edtName.Text;
            item.description = edtDescr.Text;
            return da.SpareGroupEdit(item);
        }
        private spare_group CreateItem()
        {            
                DataAccess da = new DataAccess();
                spare_group i = new spare_group();
                i.name = edtName.Text;
                i.description = edtDescr.Text;
                i.ParentGroup = da.GetSpareGroups().FirstOrDefault(g => g.name == _parentName);
                return da.SpareGroupCreate(i);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
