using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Runtime.Serialization.Json;
using S7.Net;

namespace PLC_backup
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ObservableCollection<DataBlock> _dataBlocks;
        DataBlock _datablock;
        Prop _prop;
        public MainWindow()
        {
            InitializeComponent();
            _prop = new Prop();

            //_dataBlocks = new ObservableCollection<DataBlock>();
            _datablock = new DataBlock();
            if (File.Exists(Properties.Settings.Default.Path))
            {
                LoadFromJson(Properties.Settings.Default.Path);
                Title = "PLC backup | " + Properties.Settings.Default.Path;
            }
            dg_dbbloks.ItemsSource = _prop.DataBlocks;
            SourceConection();
            setcmbitems();
        }

        private void setcmbitems()
        {
            cmb_cpy_type.Items.Add(CpuType.S7200);
            cmb_cpy_type.Items.Add(CpuType.S7300);
            cmb_cpy_type.Items.Add(CpuType.S7400);
            cmb_cpy_type.Items.Add(CpuType.S71200);
            cmb_cpy_type.Items.Add(CpuType.S71500);
        }

        private void SourceConection()
        {
            tb_ip.Text = _prop.Ip;
            tb_rack.Text = _prop.Rack.ToString();
            tb_slot.Text = _prop.Rack.ToString();
            cmb_cpy_type.SelectedItem = _prop.CpuType;
        }

        private void btn_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "json| *.json";
            openFile.CheckFileExists = true;
            openFile.CheckPathExists = true;
            if (openFile.ShowDialog() == true)
            {
                LoadFromJson(openFile.FileName);
                SourceConection();
            }
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            _prop.Ip = tb_ip.Text.Trim();
            short n =0;
            short.TryParse(tb_rack.Text.Trim(), out n);
            _prop.Rack = n;
            n = 0;
            short.TryParse(tb_slot.Text.Trim(), out n);
            _prop.Slot = n;
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "json| *.json";
            saveFile.CheckFileExists = false;
            saveFile.CheckPathExists = true;
            if (saveFile.ShowDialog() == true)
            {
                SaveToJson(saveFile.FileName);
            }
        }

        private void dg_dbbloks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_dbbloks.SelectedItems.Count > 0)
            {
                _datablock = dg_dbbloks.SelectedItems[0] as DataBlock;
                tb_name.Text = _datablock.Name;
                tb_number.Text = _datablock.Number.ToString();
                tb_size.Text = _datablock.Size.ToString();
            }
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {

            if (tb_name.Text.Trim().Count() == 0)
            {
                MessageBox.Show("Введите имя блока!");
                return;
            }
            _datablock.Name = tb_name.Text.Trim();
            int n = 0;
            if (tb_number.Text.Trim().Count() == 0)
            {
                MessageBox.Show("Введите номер блока!");
                return;
            }
            int.TryParse(tb_number.Text.Trim(), out n);
            _datablock.Number = n;
            n = 0;
            if (tb_size.Text.Trim().Count() == 0)
            {
                MessageBox.Show("Введите размер блока!");
                return;
            }
            int.TryParse(tb_size.Text.Trim(), out n);
            _datablock.Size = n;
            if (AddDataBlock())
            {
                btn_cancel_Click(sender, e);
            }
        }

        private bool AddDataBlock()
        {
            bool result = false;
            try
            {
                if (_prop.DataBlocks.Where(db => db.Name == _datablock.Name).Count() > 0)
                {
                    int i = 0;
                    i = _prop.DataBlocks.IndexOf(_prop.DataBlocks.First(db => db.Name == _datablock.Name));
                    _prop.DataBlocks[i] = new DataBlock(_datablock.Name, _datablock.Number, _datablock.Size);
                }
                else
                {
                    _prop.DataBlocks.Add(new DataBlock(_datablock.Name, _datablock.Number, _datablock.Size));
                }
                result = true;
            }
            catch (NullReferenceException)
            {
                return result;
            }
            return result;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            tb_name.Text = "";
            tb_number.Text = "";
            tb_size.Text = "";
            _datablock = new DataBlock();
        }

        private void tb_number_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btn_add_Click(null, null);
                tb_name.Focus();
            }
        }

        private bool SaveToJson(string fileName)
        {
            bool result = false;

            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Prop));

            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                jsonFormatter.WriteObject(fs, _prop);
                SaveProperty(fileName);
                result = true;
            }
            return result;
        }

        private bool LoadFromJson(string fileName)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Prop));
            bool result = false;
            ObservableCollection<DataBlock> tmp = new ObservableCollection<DataBlock>();
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                _prop = (Prop)jsonFormatter.ReadObject(fs);
                tmp = _prop.DataBlocks;
                //_prop.DataBlocks.Clear();
                for (int i = 0; i < _prop.DataBlocks.Count; i++)
                {
                    _prop.DataBlocks[i] = new DataBlock(_prop.DataBlocks[i].Name, _prop.DataBlocks[i].Number, _prop.DataBlocks[i].Size);
                }
                result = true;
            }
            SaveProperty(fileName);
            return result;
        }

        private void SaveProperty(string fileName)
        {
            Properties.Settings.Default.Path = fileName;
            Properties.Settings.Default.Save();
            Title = "PLC backup | " + Properties.Settings.Default.Path;
        }

        private void btn_backup_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savePLC = new SaveFileDialog();
            savePLC.Filter = "bin| *.bin";
            savePLC.CheckFileExists = false;
            savePLC.CheckPathExists = true;
            if (savePLC.ShowDialog() == true)
            {

            }
        }

        private bool SavePLCToBinary(string path)
        {
            bool result = false;

            return result;
        }

        private bool ReadFromPLC()
        {
            bool result = false;
            Plc plc = new Plc(_prop.CpuType, _prop.Ip,_prop.Rack ,_prop.Slot);
            try
            {
                foreach (var db in _prop.DataBlocks)
                {
                    plc.ReadBytes(DataType.DataBlock, db.Number, 0, db.Size);
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return result;
        }

        private byte[] DataBloksToBynary()
        {
            byte[] result = new byte[GetBynarySize()];
            int i = 0;
            foreach (var db in _prop.DataBlocks)
            {
                db.Data.CopyTo(result, i);
                i += db.Size;
            }
            return result;
        }

        private int GetBynarySize()
        {
            int size = 0;
            foreach (var db in _prop.DataBlocks)
            {
                size += db.Size;
            }
            return size;
        }

        private void cmb_cpy_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _prop.CpuType = (CpuType)cmb_cpy_type.SelectedItem;
        }
    }
}
