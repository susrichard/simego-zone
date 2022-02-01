using System.Windows.Forms;

namespace _TEMPLATE_NAMESPACE_PROVIDER_
{
    public partial class ConnectionInterface : UserControl
    {
        public PropertyGrid PropertyGrid { get { return propertyGrid1; } }
        
        public ConnectionInterface()
        {
            InitializeComponent();
            Setup();
        }

        public void Setup()
        {
            PropertyGrid.LineColor = System.Drawing.Color.WhiteSmoke;
        }
    }
}
