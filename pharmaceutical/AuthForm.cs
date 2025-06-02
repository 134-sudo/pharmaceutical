using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace pharmaceutical
{
    public partial class AuthForm : Form
    {
        private bool showPassword = false;
        private NpgsqlConnection conn = new NpgsqlConnection(Constants.ConnectionString);

        private Timer timer;
        private int counter = 0;
        public AuthForm()
        {
            InitializeComponent();
            login_textBox.KeyPress += textBox_KeyPress;
            password_textBox.KeyPress += textBox_KeyPress;

            timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += Timer_Tick;
            statusbar_text.Text = "";
            conn.Open();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            counter++;
            if (counter >= 3)
            {
                timer.Stop();
                statusbar_text.Text = "";
                counter = 0;
            }
        }



        private void login_button_Click(object sender, EventArgs e)
        {
            string login = login_textBox.Text;
            string password = password_textBox.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Логин и пароль должны быть заполнены!",
                                "Ошибка ввода",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                statusbar_text.Text = "Логин или пароль не может быть пустым!";
                counter = 0;
                timer.Start();
                return;
            }

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(
                    "SELECT password FROM users WHERE login = @login;", conn))
                {
                    cmd.Parameters.AddWithValue("login", login);
                    string dbPassword = cmd.ExecuteScalar()?.ToString();

                    if (dbPassword == null)
                    {
                        MessageBox.Show($"Пользователь '{login}' не найден!",
                                      "Ошибка авторизации",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning);
                        statusbar_text.Text = $"Пользователь с логином '{login}' не найден!";
                    }
                    else if (dbPassword != password)
                    {
                        MessageBox.Show("Неправильный пароль!",
                                      "Ошибка авторизации",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning);
                        statusbar_text.Text = "Неверный пароль!";
                    }
                    else
                    {
                        MessageBox.Show("Авторизация прошла успешно!",
                                      "Успех",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);
                        statusbar_text.Text = "Вход успешен!";
                        conn.Close();
                        MainForm form = new MainForm();
                        form.Show();
                        Hide();
                    }
                }
            }
            catch (NpgsqlException dbEx)
            {
                MessageBox.Show($"Ошибка базы данных: {dbEx.Message}",
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                statusbar_text.Text = $"Ошибка БД: {dbEx.Message}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                statusbar_text.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                timer.Start();
                counter = 0;
            }
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\t' || e.KeyChar == ' ' || e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                statusbar_text.Text = "Вы не можете ввести данный символ";
                counter = 0;
                timer.Start();
                e.Handled = true;
            }
        }
    }
}
