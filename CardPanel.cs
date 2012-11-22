using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows.Forms;

namespace JustCSharp
{
    [Designer(typeof(CardPanelDesigner))]
    [ToolboxItem(typeof(CardPanelToolBoxItem))]
    public class CardPanel : Control
    {
        private CardPanelPage CurrentPage;
        public event EventHandler PageChanged;
        public event EventHandler PageChanging;

        public CardPanel()
        {
            ControlAdded += new ControlEventHandler(CardPanel_ControlAdded);
            SizeChanged += new EventHandler(CardPanel_SizeChanged);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        #region Event-Handlers
        private void CardPanel_ControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control is CardPanelPage)
            {
                e.Control.Location = new Point(0, 0);
                e.Control.Size = ClientSize;
                e.Control.Dock = DockStyle.Fill;

                if (SelectedItem == null)
                {
                    SelectedItem = (CardPanelPage)e.Control;
                }
                else
                {
                    e.Control.Visible = false;
                }
            }
            else
            {
                Controls.Remove(e.Control);
            }
        }

        private void CardPanel_SizeChanged(object sender, EventArgs e)
        {
            foreach (CardPanelPage Page in Controls)
            {
                Page.Size = ClientSize;
            }
        }

        [Editor(typeof(CardPanelPageEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public CardPanelPage SelectedItem
        {
            get { return CurrentPage; }
            set
            {
                if (CurrentPage != value)
                {
                    if (PageChanging != null) { PageChanging(this, EventArgs.Empty); }
                    if (CurrentPage != null) { CurrentPage.Visible = false; }

                    CurrentPage = value;
                    CurrentPage.Visible = true;

                    if (PageChanged != null) { PageChanged(this, EventArgs.Empty); }
                }
            }
        }
        #endregion

        #region Navigation Functions
        public void NextPage()
        {
            int Index = Controls.IndexOf(CurrentPage);

            if ((Controls.Count > 1) && (Index != (Controls.Count - 1)))
            {
                CurrentPage.Visible = false;
                CurrentPage = (CardPanelPage)Controls[Index + 1];
                CurrentPage.Visible = true;
            }
        }

        public void PreviousPage()
        {
            int Index = Controls.IndexOf(CurrentPage);

            if ((Controls.Count > 1) && (Index != 0))
            {
                CurrentPage.Visible = false;
                CurrentPage = (CardPanelPage)Controls[Index - 1];
                CurrentPage.Visible = true;
            }
        }

        public void JumpToPage(int PageNumber)
        {
            if ((PageNumber >= 0) && (PageNumber < Controls.Count))
            {
                foreach(Control control in Controls)
                {
                    control.Visible = false;
                }

                CurrentPage = (CardPanelPage)Controls[PageNumber];
                CurrentPage.Visible = true;
            }
        }
        #endregion
    }

    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    [Designer(typeof(CardPanelPageDesigner))]
    public class CardPanelPage : Panel
    {
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DockStyle Dock
        {
            get { return base.Dock; }
            set { base.Dock = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Point Location
        {
            get { return base.Location; }
            set { base.Location = value; }
        }
    }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    internal class CardPanelDesigner : System.Windows.Forms.Design.ParentControlDesigner
    {
        private bool TransactionActive;
        private CardPanelPage Page;
        private DesignerVerbCollection ListOfVerbs;

        #region Contruction & Destruction

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            ISelectionService Service = (ISelectionService)GetService(typeof(ISelectionService));
            ((CardPanel)Control).PageChanged += new EventHandler(CardPanelDesigner_PageChanged);

            if (Service != null)
            {
                Service.SelectionChanged += new EventHandler(CardPanelDesigner_ComponentSelectionChanged);
            }

            IComponentChangeService ServiceChange = (IComponentChangeService)GetService(typeof(IComponentChangeService));
            if (ServiceChange != null)
            {
                ServiceChange.ComponentRemoving += new ComponentEventHandler(CardPanelDesigner_ComponentRemoving);
                ServiceChange.ComponentChanged += new ComponentChangedEventHandler(CardPanelDesigner_ComponentChanged);
            }

            ListOfVerbs = new DesignerVerbCollection(new DesignerVerb[] {
                    new DesignerVerb("Add Page", new EventHandler(CardPanelDesigner_AddPage)),
                    new DesignerVerb("Remove Page", new EventHandler(CardPanelDesigner_RemovePage))
                });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ISelectionService Service = (ISelectionService)GetService(typeof(ISelectionService));
                ((CardPanel)Control).PageChanged -= new EventHandler(CardPanelDesigner_PageChanged);

                if (Service != null)
                {
                    Service.SelectionChanged -= new EventHandler(CardPanelDesigner_ComponentSelectionChanged);
                }

                IComponentChangeService ServiceChange = (IComponentChangeService)GetService(typeof(IComponentChangeService));
                if (ServiceChange != null)
                {
                    ServiceChange.ComponentRemoving -= new ComponentEventHandler(CardPanelDesigner_ComponentRemoving);
                    ServiceChange.ComponentChanged -= new ComponentChangedEventHandler(CardPanelDesigner_ComponentChanged);
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Get & Set Functions

        public CardPanelPage SelectedItem
        {
            get { return Page; }
            set
            {
                if (Page != value)
                {
                    if (Page != null) Page.Visible = false;
                    Page = value;
                    if (Page != null) Page.Visible = true;
                }
            }
        }

        public CardPanelPageDesigner SelectedPage()
        {
            if (Page == null) return null;

            IDesignerHost DesignerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (DesignerHost != null)
            {
                return (CardPanelPageDesigner)DesignerHost.GetDesigner(Page);
            }

            return null;
        }

        private static CardPanelPage ControlParent(object control)
        {
            if (control is Control)
            {
                while ((((Control)control) != null) && (((Control)control) is CardPanelPage == false))
                {
                    control = ((Control)control).Parent;
                }

                return (CardPanelPage)control;
            }
            return null;
        }

        public override DesignerVerbCollection Verbs
        {
            get { return ListOfVerbs; }
        }

        #endregion

        #region Actions & Handlers

        private void CardPanelDesigner_AddPage(object sender, EventArgs e)
        {
            IDesignerHost DesignerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (DesignerHost != null)
            {
                Func<IDesignerHost, object, object> TransactionFunction = (Host, Param) =>
                {
                    CardPanelPage NewPage = (CardPanelPage)Host.CreateComponent(typeof(CardPanelPage));
                    MemberDescriptor Description = TypeDescriptor.GetProperties((CardPanel)Control)["Controls"];
                    RaiseComponentChanging(Description);
                    ((CardPanel)Control).Controls.Add(NewPage);
                    RaiseComponentChanged(Description, null, null);
                    return null;
                };

                TransactionActive = true;
                TransactionInfo("Add Page", DesignerHost, TransactionFunction);
                TransactionActive = false;
            }
        }

        private void CardPanelDesigner_RemovePage(object sender, EventArgs e)
        {
            IDesignerHost DesignerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (DesignerHost != null)
            {
                Func<IDesignerHost, object, object> TransactionFunction = (Host, Param) =>
                {
                    if (Page == null) return null;
                    MemberDescriptor Description = TypeDescriptor.GetProperties((CardPanel)Control)["Controls"];
                    RaiseComponentChanging(Description);
                    try
                    {
                        DesignerHost.DestroyComponent(Page);
                    }
                    catch (Exception Ex)
                    {
                        Ex.ToString();
                    }
                    RaiseComponentChanged(Description, null, null);
                    return null;
                };

                TransactionActive = true;
                TransactionInfo("Remove Page", DesignerHost, TransactionFunction);
                TransactionActive = false;
            }
        }

        private void CardPanelDesigner_PageChanged(object sender, EventArgs e)
        {
            Page = ((CardPanel)Control).SelectedItem;
        }

        private void CardPanelDesigner_ComponentSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService Service = (ISelectionService)GetService(typeof(ISelectionService));
            if (Service != null)
            {
                ICollection SelectedComponents = Service.GetSelectedComponents();
                foreach (object SelectedComponent in SelectedComponents)
                {
                    CardPanelPage CurrentPage = ControlParent(SelectedComponent);
                    if ((CurrentPage != null) && (CurrentPage.Parent == (CardPanel)Control))
                    {
                        SelectedItem = CurrentPage;
                        break;
                    }
                }
            }
        }

        private void CardPanelDesigner_ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (Control == null)
            {
                foreach (DesignerVerb Verb in ListOfVerbs)
                {
                    Verb.Enabled = false;
                }
            }
            else
            {
                ListOfVerbs[0].Enabled = true;
                ListOfVerbs[1].Enabled = (Control.Controls.Count > 1);
            }
        }

        private void CardPanelDesigner_ComponentRemoving(object sender, ComponentEventArgs e)
        {
            if (e.Component is CardPanelPage)
            {
                if (((CardPanel)Control).Contains((CardPanelPage)e.Component))
                {
                    IDesignerHost DesignerHost = (IDesignerHost)GetService(typeof(IDesignerHost));

                    Func<IDesignerHost, object, object> TransactionFunction = (Host, Param) =>
                    {
                        int Index = ((CardPanel)Control).Controls.IndexOf(Page);

                        if (Page == ((CardPanel)Control).SelectedItem)
                        {
                            MemberDescriptor Description = TypeDescriptor.GetProperties((CardPanel)Control)["SelectedItem"];
                            RaiseComponentChanging(Description);
                            if (((CardPanel)Control).Controls.Count > 1)
                            {
                                ((CardPanel)Control).SelectedItem = (Index == ((CardPanel)Control).Controls.Count - 1) ? (CardPanelPage)((CardPanel)Control).Controls[Index - 1] : (CardPanelPage)((CardPanel)Control).Controls[Index + 1];
                            }
                            else
                            {
                                ((CardPanel)Control).SelectedItem = null;
                            }
                            RaiseComponentChanged(Description, null, null);
                        }
                        else
                        {
                            if (((CardPanel)Control).Controls.Count > 1)
                            {
                                SelectedItem = (Index == ((CardPanel)Control).Controls.Count - 1) ? (CardPanelPage)((CardPanel)Control).Controls[Index - 1] : (CardPanelPage)((CardPanel)Control).Controls[Index + 1];
                            }
                            else
                            {
                                SelectedItem = null;
                            }
                        }
                        return null;
                    };

                    if (!TransactionActive)
                    {
                        TransactionActive = true;
                        TransactionInfo("ComponentRemoving", DesignerHost, TransactionFunction);
                        TransactionActive = false;
                    }
                    else
                    {
                        TransactionFunction(DesignerHost, null);
                    }

                    if (Control == null)
                    {
                        foreach (DesignerVerb Verb in ListOfVerbs)
                        {
                            Verb.Enabled = false;
                        }
                    }
                    else
                    {
                        ListOfVerbs[0].Enabled = true;
                        ListOfVerbs[1].Enabled = (Control.Controls.Count > 1);
                    }
                }
            }
        }

        private static object TransactionInfo(string TransactionName, IDesignerHost DesignerHost, Func<IDesignerHost, object, object> TransactionFunction)
        {
            object Value = null;
            DesignerTransaction Transaction = null;
            try
            {
                if (DesignerHost != null)
                {
                    Transaction = DesignerHost.CreateTransaction(TransactionName);
                    Value = TransactionFunction.Invoke(DesignerHost, null);
                }
            }
            catch (Exception Ex)
            {
                Ex.ToString();

                if (Transaction != null)
                {
                    Transaction.Cancel();
                    Transaction = null;
                }
            }
            finally
            {
                if (Transaction != null)
                {
                    Transaction.Commit();
                }
            }

            return Value;
        }

        #endregion

        #region Overloads

        public override bool CanParent(Control control)
        {
            if (control is CardPanelPage)
                return !Control.Contains(control);

            return false;
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            CardPanelPageDesigner DesignerPage = SelectedPage();
            if (DesignerPage != null)
            {
                DesignerPage.CardPanelPageDesigner_OnGiveFeedback(e);
            }
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            CardPanelPageDesigner DesignerPage = SelectedPage();
            if (DesignerPage != null)
            {
                DesignerPage.CardPanelPageDesigner_OnDragEnter(de);
            }
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            CardPanelPageDesigner DesignerPage = SelectedPage();
            if (DesignerPage != null)
            {
                DesignerPage.CardPanelPageDesigner_OnDragDrop(de);
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            CardPanelPageDesigner DesignerPage = SelectedPage();
            if (DesignerPage != null)
            {
                DesignerPage.CardPanelPageDesigner_OnDragLeave(e);
            }
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            System.Drawing.Point Location = ((CardPanel)Control).PointToClient(new System.Drawing.Point(de.X, de.Y));
            if (!((CardPanel)Control).DisplayRectangle.Contains(Location))
            {
                de.Effect = DragDropEffects.None;
            }
            else
            {
                CardPanelPageDesigner DesignerPage = SelectedPage();
                if (DesignerPage != null)
                {
                    DesignerPage.CardPanelPageDesigner_OnDragOver(de);
                }
            }
        }

        #endregion
    }

    internal class CardPanelPageDesigner : System.Windows.Forms.Design.ScrollableControlDesigner
    {
        /*TODO: Add Painting */
        private int X = -1, Y = -1;
        private bool MouseMoving = false;

        #region Get & Set Functions
        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                if (Control.Parent is CardPanel)
                {
                    System.Windows.Forms.Design.SelectionRules Rules = base.SelectionRules;
                    return Rules &= ~System.Windows.Forms.Design.SelectionRules.AllSizeable;
                }
                return base.SelectionRules;
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                DesignerVerbCollection ListOfVerbs = new DesignerVerbCollection();
                foreach (DesignerVerb Verb in base.Verbs)
                {
                    ListOfVerbs.Add(Verb);
                }


                if ((Control != null) && (Control.Parent != null))
                {
                    IDesignerHost DesignerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
                    if (DesignerHost != null)
                    {
                        CardPanelDesigner PanelDesigner = (CardPanelDesigner)DesignerHost.GetDesigner(Control.Parent);
                        if (PanelDesigner != null)
                        {
                            foreach (DesignerVerb Verb in PanelDesigner.Verbs)
                            {
                                ListOfVerbs.Add(Verb);
                            }
                        }
                    }
                }

                return ListOfVerbs;
            }
        }

        public override bool CanBeParentedTo(IDesigner parentDesigner)
        {
            if (parentDesigner != null)
            {
                return (parentDesigner.Component is CardPanel);
            }

            return false;
        }
        #endregion

        #region Mouse Overrides

        protected override bool GetHitTest(System.Drawing.Point point)
        {
            return false;
        }

        protected override void OnMouseDragBegin(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        protected override void OnMouseDragMove(int x, int y)
        {
            int Offset = 1;
            if ((x > this.X + Offset) || (x < this.X - Offset) || (y > this.Y + Offset) || (y < this.Y - Offset))
            {
                MouseMoving = true;
                base.OnMouseDragBegin(this.X, this.Y);
                base.OnMouseDragMove(x, y);
            }
        }

        protected override void OnMouseDragEnd(bool cancel)
        {
            bool Ready = (!MouseMoving && Control.Parent != null);
            if (Ready)
            {
                ISelectionService Service = (ISelectionService)GetService(typeof(ISelectionService));
                if (Service != null)
                {
                    Service.SetSelectedComponents(new Control[] { Control.Parent });
                }
                else
                {
                    Ready = false;
                }
            }

            if (!Ready)
            {
                base.OnMouseDragEnd(cancel);
            }

            MouseMoving = false;
        }

        #endregion

        #region Actions & Handlers
        internal void CardPanelPageDesigner_OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            OnGiveFeedback(e);
        }

        internal void CardPanelPageDesigner_OnDragEnter(DragEventArgs de)
        {
            OnDragEnter(de);
        }

        internal void CardPanelPageDesigner_OnDragDrop(DragEventArgs de)
        {
            OnDragDrop(de);
        }

        internal void CardPanelPageDesigner_OnDragLeave(EventArgs e)
        {
            OnDragLeave(e);
        }

        internal void CardPanelPageDesigner_OnDragOver(DragEventArgs de)
        {
            OnDragOver(de);
        }
        #endregion
    }

    internal class CardPanelPageEditor : ObjectSelectorEditor
    {
        protected override void FillTreeWithData(ObjectSelectorEditor.Selector selector, ITypeDescriptorContext context, IServiceProvider provider)
        {
            base.FillTreeWithData(selector, context, provider);
            CardPanel Panel = (CardPanel)context.Instance;
            foreach (CardPanelPage Page in Panel.Controls)
            {
                SelectorNode Node = new SelectorNode(Page.Name, Page);
                selector.Nodes.Add(Node);
                if (Page == Panel.SelectedItem)
                {
                    selector.SelectedNode = Node;
                }
            }
        }
    }

    [Serializable]
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    internal class CardPanelToolBoxItem : ToolboxItem
    {
        public CardPanelToolBoxItem() : base(typeof(CardPanel)) { }

        public CardPanelToolBoxItem(SerializationInfo Info, StreamingContext Context)
		{
			Deserialize(Info, Context);
		}

        #region Component Creation & Transaction Functions
        protected override IComponent[] CreateComponentsCore(IDesignerHost DesignerHost)
		{
            Func<IDesignerHost, object, object> TransactionFunction = (Host, Param) =>
            {
                CardPanel Panel = (CardPanel)Host.CreateComponent(typeof(CardPanel));
                if (Panel != null)
                {
                    CardPanelPage Page = (CardPanelPage)Host.CreateComponent(typeof(CardPanelPage));
                    if (Page != null)
                    {
                        Panel.Controls.Add(Page);
                        return new IComponent[] { Panel };
                    }
                }
                return null;
            };

            return TransactionInfo("CardPanel_ToolBoxItemCreated", DesignerHost, TransactionFunction) as IComponent[];
		}

        private static object TransactionInfo(string TransactionName, IDesignerHost DesignerHost, Func<IDesignerHost, object, object> TransactionFunction)
        {
            object Value = null;
            DesignerTransaction Transaction = null;
            try
            {
                if (DesignerHost != null)
                {
                    Transaction = DesignerHost.CreateTransaction(TransactionName);
                    Value = TransactionFunction.Invoke(DesignerHost, null);
                }
            }
            catch (Exception Ex)
            {
                Ex.ToString();

                if (Transaction != null)
                {
                    Transaction.Cancel();
                    Transaction = null;
                }
            }
            finally
            {
                if (Transaction != null)
                {
                    Transaction.Commit();
                }
            }

            return Value;
        }
        #endregion
    }
}