﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using bycar;
using bycar3.Core;
using bycar3.Views.Common;
using bycar3.Views.Main_window;

namespace bycar3.Views
{
    /// <summary>
    /// Interaction logic for SpareEditView.xaml
    /// </summary>
    public partial class SpareEditView : Window
    {
        public int _id = 0;
        public string _oldName = "";
        public spare _spare = null;

        private string OldBrandName = "";

        //string OldGroupName = "";
        private int OldGroupID = 0;

        private bool IsNew = true;
        private bool TemporarySaved = false;

        public UCSpares ParentWorkspace = null;
        public SpareView SpareViewItem = null;

        #region FUNCTIONS CUSTOM

        public void LoadItem(int id)
        {
            DataAccess da = new DataAccess();
            _spare = da.GetSpare(id);
            if (_spare != null)
            {
                IsNew = false;
                if (_spare.brand == null)
                    _spare.brandReference.Load();
                if (_spare.spare_group == null)
                    _spare.spare_groupReference.Load();
                if (_spare.unit == null)
                    _spare.unitReference.Load();
                _id = _spare.id;
                string Name = _spare.name;
                string Code = _spare.code;
                string CodeShatem = _spare.codeShatem;
                string QDemand = _spare.q_demand.ToString();
                int GroupID = _spare.spare_group.id;
                int BrandID = _spare.brand.id;
                int UnitID = _spare.unit.id;
                string BrandName = _spare.brand.name;
                string GroupName = _spare.spare_group.name;
                string UnitName = _spare.unit.name;
                string Description = _spare.description;
                OldBrandName = BrandName;
                OldGroupID = GroupID;

                edtName.Text = Name;
                edtCode.Text = Code;
                edtCodeShatem.Text = CodeShatem;
                edtQ_Demand.Text = QDemand;
                edtGroup.SelectedValue = GroupID;
                edtBrand.SelectedValue = BrandID;
                edtUnit.SelectedValue = UnitID;
                edtDescr.Text = Description;
            }
        }

        #endregion FUNCTIONS CUSTOM

        public SpareEditView()
        {
            InitializeComponent();
            loadComboBox_Units();
            loadComboBox_Brands();
            loadComboBox_SpareGroups();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this._id > 0)
            {
                _spare = EditItem();
                TemporarySaved = false;
                this.Close();
            }
            else
            {
                DataAccess da = new DataAccess();
                if (!da.CodeExist(edtCode.Text) || edtCode.Text.Equals(""))
                {
                    _spare = CreateItem();
                    if (_spare == null)
                        return;
                    TemporarySaved = false;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Такой код магазина уже существует!");
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool Validated()
        {
            if (edtBrand.SelectedItem == null)
            {
                MessageBox.Show("Выберите брэнд!");
                return false;
            }
            if (edtGroup.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу!");
                return false;
            }
            return true;
        }

        private spare CreateItem()
        {
            if (!Validated())
                return null;

            string Name = edtName.Text;
            string Code = edtCode.Text;
            string CodeShatem = edtCodeShatem.Text;
            int QDemand = 0;
            Int32.TryParse(edtQ_Demand.Text, out QDemand);
            int GroupID = (int)edtGroup.SelectedValue;
            int BrandID = (int)edtBrand.SelectedValue;
            int UnitID = (int)edtUnit.SelectedValue;
            string Description = edtDescr.Text;
            return Marvin.Instance.SpareCreate(Name, Code, CodeShatem, QDemand, GroupID, BrandID, UnitID, Description);
        }

        private spare EditItem()
        {
            if (!Validated())
                return null;
            string Name = edtName.Text;
            string Code = edtCode.Text;
            string CodeShatem = edtCodeShatem.Text;
            int QDemand = 0;
            Int32.TryParse(edtQ_Demand.Text, out QDemand);
            int GroupID = (int)edtGroup.SelectedValue;
            int BrandID = (int)edtBrand.SelectedValue;
            int UnitID = (int)edtUnit.SelectedValue;
            string Description = edtDescr.Text;
            return Marvin.Instance.SpareEdit(this._id, Name, Code, CodeShatem, QDemand, GroupID, BrandID, UnitID, Description);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadAnalogues(this._id);
            _oldName = edtName.Text;
        }

        private void loadComboBox_Units()
        {
            DataAccess da = new DataAccess();
            List<unit> items = da.GetUnits();
            edtUnit.DataContext = items;
            edtUnit.SelectedIndex = 0;
        }

        private void loadComboBox_Brands()
        {
            DataAccess da = new DataAccess();
            List<brand> items = da.GetBrands();
            edtBrand.DataContext = items;
        }

        private void loadComboBox_SpareGroups()
        {
            DataAccess da = new DataAccess();
            List<spare_group> items = da.GetOnlySpareGroups();
            edtGroup.DataContext = items;
        }

        private void loadAnalogues(int spareId)
        {
            DataAccess da = new DataAccess();
            List<SpareView> items = da.GetAnalogues(spareId);
            dgAnalogues.DataContext = items;
            dgAnalogues.SelectedIndex = 0;
        }

        private void CallSelectWindowBrand()
        {
            SelectView v = new SelectView();
            v.ClassName = (new brand()).ToString();
            v.ShowDialog();
            loadComboBox_Brands();

            //edtBrand.SelectedItem = v.RES;
            if (v.Selected != null)
                edtBrand.SelectedValue = v.Selected._Id;
        }

        private void CallSelectWindowGroup()
        {
            SelectView v = new SelectView();
            v.ClassName = (new spare_group()).ToString();
            v.ShowDialog();
            loadComboBox_SpareGroups();

            //edtGroup.SelectedItem = v.RES;
            if (v.Selected != null)
                edtGroup.SelectedValue = v.Selected._Id;
        }

        private void CallSelectWindowUnit()
        {
            SelectView v = new SelectView();
            v.ClassName = (new unit()).ToString();
            v.ShowDialog();
            loadComboBox_Units();

            //edtUnit.SelectedItem = v.RES;
            if (v.Selected != null)
                edtUnit.SelectedValue = v.Selected._Id;
        }

        private void DeleteSelectedAnalogue()
        {
            int id = 0;
            SpareView b = null;
            if (dgAnalogues.SelectedItem != null)
            {
                object sel = dgAnalogues.SelectedItem;
                b = (SpareView)sel;
                id = b.id;
                string spareName1 = _oldName;
                string spareName2 = b.name;
                DataAccess da = new DataAccess();
                spare_analogue sp = da.getSpareAnalogue(spareName1, spareName2);

                MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выделенную запись?", "Удаление", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    da.SpareAnalogueDelete(sp.id);
                    loadAnalogues(this._id);
                }
            }
        }

        private void EditSelectedAnalogue()
        {
            int id = 0;
            SpareView b = null;
            if (dgAnalogues.SelectedItem != null)
            {
                object sel = dgAnalogues.SelectedItem;
                b = (SpareView)(sel);
                id = b.id;
                string spareName1 = this._oldName;
                string spareName2 = b.name;
                if (id > 0)
                {
                    SpareAnalogueEditView v = new SpareAnalogueEditView();
                    DataAccess da = new DataAccess();
                    spare_analogue sp = da.getSpareAnalogue(spareName1, spareName2);
                    v._id = sp.id;
                    v._spareId1 = _id;
                    v._spareId2 = b.id; ;
                    v.edtIsBoth.IsChecked = sp.is_equal > 0 ? true : false;
                    v.result = b;
                    v.ShowDialog();
                    loadAnalogues(this._id);
                }
            }
        }

        private void CreateAnalogue()
        {
            SpareAnalogueEditView v = new SpareAnalogueEditView();
            v._id = -1;
            v._spareId1 = this._id;
            v.ShowDialog();
            loadAnalogues(this._id);
        }

        private void btnSpareAnalogueAdd_Click(object sender, RoutedEventArgs e)
        {
            bool canCreateAnalogue = true;
            if (this._id < 1)
            {
                // СОХРАНИТЬ ЗАПИСЬ О ЗАПЧАСТИ
                if (edtName.Text.Length < 1)
                {
                    MessageBox.Show("Сначала укажите наименование запчасти!");
                    canCreateAnalogue = false;
                }
                else
                {
                    // создать запись
                    // получить ID
                    this._id = CreateItem().id;
                    if (IsNew)
                        TemporarySaved = true;
                }
            }
            if (canCreateAnalogue)
                CreateAnalogue();
        }

        private void btnSpareAnalogueEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedAnalogue();
        }

        private void btnSpareAnalogueDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedAnalogue();
        }

        private void dgAnalogues_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSelectedAnalogue();
        }

        private void btnSelectBrand_Click(object sender, RoutedEventArgs e)
        {
            CallSelectWindowBrand();
        }

        private void btnSelectGroup_Click(object sender, RoutedEventArgs e)
        {
            CallSelectWindowGroup();
        }

        private void btnSelectUnit_Click(object sender, RoutedEventArgs e)
        {
            CallSelectWindowUnit();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsNew && TemporarySaved)
            {
                DataAccess db = new DataAccess();
                SpareViewItem = db.GetSpareView(this._id);
                Marvin.Instance.SpareDelete(SpareViewItem);
            }
        }
    }
}