using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MFilesAPI;

namespace BimIfcUploader
{
    public partial class PartForm : Form
    {
        private readonly Vault _vault;
        public PartForm(Vault vault)
        {
            InitializeComponent();
            _vault = vault;
        }
    }
}
