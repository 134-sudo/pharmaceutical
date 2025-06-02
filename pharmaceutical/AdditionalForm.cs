using Npgsql;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace pharmaceutical
{
    public partial class AdditionalForm : Form
    {
        private MainForm mainForm;
        private const int BaseCashback = 10;
        private const int DiscountCashback = 20;

        public AdditionalForm(MainForm mainForm)
        {
            InitializeComponent();
            InitializeHeader();
            this.mainForm = mainForm;
            LoadPartnersWithDiscounts(); // Заменено с LoadVisitorsWithCashback()
        }

        private void InitializeHeader()
        {
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Pink,
            };
        }

        private void LoadPartnersWithDiscounts()
        {
            const string query = @"
        SELECT 
            p.partner_id,
            p.partner_type || ' ' || p.partner_name AS partner_display_name,
            p.contact_person,
            p.phone,
            p.rating,
            COALESCE(SUM(s.total_amount), 0) AS total_sales,
            CASE 
                WHEN COALESCE(SUM(s.total_amount), 0) > 300000 THEN 15
                WHEN COALESCE(SUM(s.total_amount), 0) > 50000 THEN 10
                WHEN COALESCE(SUM(s.total_amount), 0) > 10000 THEN 5
                ELSE 0
            END AS discount_percent
        FROM partners p
        LEFT JOIN sales s ON p.partner_id = s.partner_id
        GROUP BY p.partner_id
        ORDER BY p.rating DESC";

            try
            {
                using var conn = new NpgsqlConnection(Constants.ConnectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                int yPos = 20;
                while (reader.Read())
                {
                    var card = CreatePartnerCard(
                        partnerDisplayName: reader["partner_display_name"].ToString(),
                        contactPerson: reader["contact_person"].ToString(),
                        phone: reader["phone"].ToString(),
                        rating: Convert.ToInt32(reader["rating"]),
                        discountPercent: Convert.ToInt32(reader["discount_percent"]),
                        top: yPos
                    );
                    items_panel.Controls.Add(card);
                    yPos += card.Height + 15;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private Panel CreatePartnerCard(string partnerDisplayName, string contactPerson, string phone, int rating, int discountPercent, int top)
        {
            var card = new Panel
            {
                Size = new Size(720, 100),
                Location = new Point(20, top),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };

            var nameLabel = new Label
            {
                Text = $"{partnerDisplayName}    {discountPercent}%",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var contactLabel = new Label
            {
                Text = contactPerson,
                Location = new Point(10, 35),
                AutoSize = true
            };

            var phoneLabel = new Label
            {
                Text = phone,
                Location = new Point(10, 60),
                AutoSize = true
            };

            var ratingLabel = new Label
            {
                Text = $"Рейтинг: {rating}",
                Location = new Point(500, 10),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { nameLabel, contactLabel, phoneLabel, ratingLabel });
            return card;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            mainForm?.Show();
        }

        private void go_back_button_Click(object sender, EventArgs e) => Close();
    }
}