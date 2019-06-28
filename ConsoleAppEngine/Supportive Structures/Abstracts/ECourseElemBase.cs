﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace ConsoleAppEngine.Abstracts
{
    public abstract class EElementBase<T> where T : EElementItemBase
    {
        internal readonly LinkedList<T> lists = new LinkedList<T>();
        protected T ItemToChange { get; set; }

        protected Grid ViewGrid;
        protected Grid AddGrid;
        protected ListView ViewList;
        protected Button AddButton;
        protected readonly ContentDialog contentDialog = new ContentDialog()
        {
            PrimaryButtonText = "Modify",
            SecondaryButtonText = "Delete",
            CloseButtonText = "Ok"
        };
        protected AppBarButton ViewCommand;
        protected AppBarButton AddCommand;

        protected abstract void AddNewItem();
        protected abstract Grid Header();
        protected abstract void InitializeAddGrid(params FrameworkElement[] AddViewGridControls);
        protected abstract void CheckInputs(LinkedList<Control> Controls, LinkedList<Control> ErrorWaale);
        protected abstract void ClearAddGrid();
        protected abstract void ItemToChangeUpdate();
        protected abstract void SetContentDialog();
        protected abstract void SetAddGrid_ItemToChange();
        protected abstract IOrderedEnumerable<T> OrderList();
        public abstract void DestructViews();

        public virtual void PostDeleteTasks()
        {

        }

        public void InitializeViews(Grid viewGrid, Grid addGrid, AppBarButton viewCommand, AppBarButton addCommand, params FrameworkElement[] AddViewGridControls)
        {
            ViewGrid = viewGrid;
            AddGrid = addGrid;
            ViewCommand = viewCommand;
            AddCommand = addCommand;

            FillViewGrid();
            InitializeAddGrid(AddViewGridControls);
            SetEvents();

            ViewGrid.Visibility = Visibility.Visible;
            AddGrid.Visibility = Visibility.Collapsed;
        }

        private void SetEvents()
        {
            ViewCommand.Click += (sender, e) =>
            {
                AddGrid.Visibility = Visibility.Collapsed;
                ViewGrid.Visibility = Visibility.Visible;
                if (AddButton.Content.ToString() == "Modify")
                {
                    ClearAddGrid();
                }
            };
            AddCommand.Click += (sender, e) =>
            {
                ViewGrid.Visibility = Visibility.Collapsed;
                AddGrid.Visibility = Visibility.Visible;
                ClearAddGrid();
            };
            AddButton.Click += (sender, e) =>
            {
                try
                {
                    AbstractCheckInputs();
                }
                catch
                {
                    return;
                }
                if (AddButton.Content.ToString() == "Add")
                {
                    AddNewItem();
                }
                else if (AddButton.Content.ToString() == "Modify")
                {
                    ItemToChangeUpdate();
                    UpdateList();
                }
                ViewGrid.Visibility = Visibility.Visible;
                AddGrid.Visibility = Visibility.Collapsed;
                ClearAddGrid();
                ItemToChange = null;
                return;
            };
            ViewList.SelectionChanged += async (sender, e) =>
            {
                if (ViewList.SelectedItem == null)
                {
                    return;
                }

                foreach (var a in lists)
                {
                    if (a.GetView == ViewList.SelectedItem)
                    {
                        ItemToChange = a;
                        break;
                    }
                }

                ViewList.SelectedItem = null;

                if (ItemToChange.PointerOverObject != null &&
                ItemToChange.PointerOverObject is ButtonBase x && x.IsPointerOver)
                {
                    return;
                }

                SetContentDialog();

                switch (await contentDialog.ShowAsync())
                {
                    // DELETE
                    case ContentDialogResult.Secondary:
                        ViewList.Items.Remove(ItemToChange.GetView);
                        lists.Remove(ItemToChange);
                        ItemToChange.IsDeleted = true;
                        PostDeleteTasks();
                        ItemToChange = null;
                        break;

                    // MODIFY
                    case ContentDialogResult.Primary:

                        ViewGrid.Visibility = Visibility.Collapsed;
                        AddGrid.Visibility = Visibility.Visible;

                        AddButton.Content = "Modify";
                        SetAddGrid_ItemToChange();
                        break;
                }
            };
        }

        private void AbstractCheckInputs()
        {
            LinkedList<Control> controls_cando = new LinkedList<Control>();

            LinkedList<Control> controls_err = new LinkedList<Control>();

            CheckInputs(controls_cando, controls_err);

            foreach (Control x in controls_cando)
            {
                x.BorderBrush = new SolidColorBrush(Color.FromArgb(102, 255, 255, 255));
            }

            AddButton.BorderBrush = new SolidColorBrush(Color.FromArgb(102, 255, 255, 255));

            foreach (Control x in controls_err)
            {
                x.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            }

            if (controls_err.Count != 0)
            {
                AddButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                throw new Exception();
            }
        }

        protected void FillViewGrid()
        {
            Grid header = Header();
            Grid.SetRow(header, 0);
            ViewList = new ListView();
            Grid.SetRow(ViewList, 1);

            ViewGrid.Children.Add(header);
            ViewGrid.Children.Add(ViewList);

            UpdateList();
        }

        protected void UpdateList()
        {
            if (ViewList == null)
            {
                return;
            }

            foreach (var a in (from x in lists where x.IsDeleted == true select x).ToArray())
            {
                lists.Remove(a);
            }

            List<T> v = OrderList().ToList();
            lists.Clear();
            foreach (var x in v)
            {
                lists.AddLast(x);
            }

            ViewList.Items.Clear();

            foreach (var a in from a in lists select a.GetView)
            {
                ViewList.Items.Add(a);
            }
        }

        protected static Grid GenerateHeader(params (string Name, double Width)[] Input)
        {
            Grid grid = new Grid() { Margin = new Thickness(10, 10, 10, 10) };
            int i = 0;
            foreach ((string Name, double Width) x in Input)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(x.Width, GridUnitType.Star) });
                TextBlock temp = new TextBlock()
                {
                    Text = x.Name,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetColumn(temp, i++);
                grid.Children.Add(temp);
            }

            return grid;
        }
    }
}