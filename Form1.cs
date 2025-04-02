using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace 串口助手__my
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;//取消跨线程检查
        }

        public void UpdateTextBoxForHexInput(TextBox textBox)
        {
            // 订阅 TextBox 的 TextChanged 事件
            //textBox.TextChanged += (sender, e) =>
            //{
            // 获取当前 TextBox 的文本
            string input = textBox.Text;

            // 使用正则表达式移除非十六进制字符
            string hexOnly = Regex.Replace(input, "[^0-9A-Fa-f]", "");

            // 创建一个新的 StringBuilder 对象来构建格式化后的字符串
            StringBuilder formatted = new StringBuilder();

            // 遍历十六进制字符串，按字节分割
            for (int i = 0; i < hexOnly.Length; i += 2)
            {
                // 检查剩余字符数量
                int byteLength = (hexOnly.Length - i) >= 2 ? 2 : 1;

                // 添加当前字节到格式化字符串
                formatted.Append(hexOnly.Substring(i, byteLength));

                // 如果不是最后一个字节，添加一个空格作为分隔符
                if (i + byteLength < hexOnly.Length)
                {
                    formatted.Append(" ");
                }
            }

            // 更新 TextBox 的文本
            textBox.Text = formatted.ToString();

            // 设置光标位置到文本末尾
            textBox.SelectionStart = textBox.Text.Length;
            //};
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox2.Text;//串口号
                serialPort1.BaudRate = Convert.ToInt32(comboBox1.Text);//波特率
                serialPort1.Open();//打开串口
                button1.Enabled = false;//禁用打开串口按钮
                button2.Enabled = true;//启用关闭串口按钮
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();//关闭串口
                button1.Enabled = true;//启用打开串口按钮
                button2.Enabled = false;//禁用关闭串口按钮
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//串口没有打开
            {
                MessageBox.Show("串口未打开", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;//启用打开串口按钮
                button2.Enabled = false;//禁用关闭串口按钮
                return;
            }
            if (textBox2.Text == "")//发送区为空
            {
                MessageBox.Show("发送区为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!radioButton1.Checked)//发送模式为字符模式
            {
                try
                {
                    serialPort1.Write(textBox2.Text);//发送数据
                }
                catch (Exception ex)
                {
                    serialPort1.Close();//关闭串口
                    button1.Enabled = true;//启用打开串口按钮
                    button2.Enabled = false;//禁用关闭串口按钮
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else//发送模式为十六进制模式
            {
                try
                {
                    byte[] data = new byte[(textBox2.Text.Length / 3) + 1];//发送字节数组
                    for (int i = 0; i < data.Length; i++)
                    {
                        // 检查剩余字符数量
                        int byteLength = (textBox2.Text.Length - i * 3) >= 2 ? 2 : 1;
                        data[i] = Convert.ToByte(textBox2.Text.Substring(i * 3, byteLength), 16);//将十六进制字符串转换为字节数
                    }
                    serialPort1.Write(data, 0, data.Length);//发送数据
                }
                catch (Exception ex)
                {
                    serialPort1.Close();//关闭串口
                    button1.Enabled = true;//启用打开串口按钮
                    button2.Enabled = false;//禁用关闭串口按钮
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Clear();//清空接收区
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox2.Clear();//清空发送区
        }

        private void button6_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[6];//发送字节数组
            data[0] = 0x55;//帧头
            if (!serialPort1.IsOpen)//串口没有打开
            {
                MessageBox.Show("串口未打开", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;//启用打开串口按钮
                button2.Enabled = false;//禁用关闭串口按钮
                return;
            }
            if (comboBox3.Text == "")//没有选择I2C模式
            {
                MessageBox.Show("没有选择I2C模式", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (comboBox3.Text == "I2C_8bit")
            {
                data[1] = 0x02;//选择I2C_8bit模式
                data[2] = 0x03;//写一个字节模式
                if (textBox3.Text == "")//没有输入地址
                {
                    MessageBox.Show("没有输入地址", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (textBox3.Text.Length > 2)//地址长度大于2
                {
                    MessageBox.Show("地址长度大于2", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    data[3] = Convert.ToByte(textBox3.Text, 16);//将十六进制字符串转换为字节数
                }


                data[4] = 0x01;//读写一个字节


                if (textBox4.Text == "")//没有输入寄存器地址
                {
                    MessageBox.Show("没有输入寄存器地址", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (textBox4.Text.Length > 2)//寄存器地址长度大于2
                {
                    MessageBox.Show("寄存器地址长度大于2，请选择I2C_16bit", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    data[5] = Convert.ToByte(textBox4.Text, 16);//将十六进制字符串转换为字节数
                }
                try
                {
                    serialPort1.Write(data, 0, data.Length);//发送数据
                }
                catch (Exception ex)
                {
                    MessageBox.Show("发送失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[7];//发送字节数组
            data[0] = 0x55;//帧头
            if (!serialPort1.IsOpen)//串口没有打开
            {
                MessageBox.Show("串口未打开", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;//启用打开串口按钮
                button2.Enabled = false;//禁用关闭串口按钮
                return;
            }
            if (comboBox3.Text == "")//没有选择I2C模式
            {
                MessageBox.Show("没有选择I2C模式", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (comboBox3.Text == "I2C_8bit")
            {
                data[1] = 0x02;//选择I2C_8bit模式
                data[2] = 0x01;//写一个字节模式
                if (textBox3.Text == "")//没有输入地址
                {
                    MessageBox.Show("没有输入地址", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (textBox3.Text.Length > 2)//地址长度大于2
                {
                    MessageBox.Show("地址长度大于2", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    data[3] = Convert.ToByte(textBox3.Text, 16);//将十六进制字符串转换为字节数
                }


                data[4] = 0x01;//读写一个字节


                if (textBox4.Text == "")//没有输入寄存器地址
                {
                    MessageBox.Show("没有输入寄存器地址", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (textBox4.Text.Length > 2)//寄存器地址长度大于2
                {
                    MessageBox.Show("寄存器地址长度大于2，请选择I2C_16bit", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    data[5] = Convert.ToByte(textBox4.Text, 16);//将十六进制字符串转换为字节数
                }
                if (textBox5.Text == "")//没有输入数据
                {
                    MessageBox.Show("没有输入数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }
                else if (textBox5.Text.Length > 2)//数据长度大于2
                {
                    MessageBox.Show("数据长度大于2", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    data[6] = Convert.ToByte(textBox5.Text, 16);//将十六进制字符串转换为字节数
                }
                try
                {
                    serialPort1.Write(data, 0, data.Length);//发送数据
                }
                catch (Exception ex)
                {
                    MessageBox.Show("发送失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }



            }
            else if (comboBox3.Text == "I2C_16bit")
            {

            }
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox24_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for(int i = 1; i < 21; i++)
            {
                try
                {
                    serialPort1.PortName = "COM" + i.ToString();
                    serialPort1.Open();
                    comboBox2.Items.Add("COM" + i.ToString());
                    serialPort1.Close();
                    comboBox2.Text = "COM"+i.ToString();//默认COM
                }catch
                {
                    continue;
                }

                //comboBox2.Items.Add("COM"+i.ToString());
            }
            //comboBox2.Text = "COM1";//默认COM1
            comboBox1.Text = "115200";//默认115200
            comboBox3.Text = "I2C_8bit";//默认I2C_8bit
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(Port1_DataReceived);

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(!radioButton4.Checked)//接收模式为字符模式
            {
                string str = serialPort1.ReadExisting();//读取数据
                textBox1.AppendText(str);//显示数据
                //以上代码无法显示中文
                string pattern_I2C = @"Return Value = 0x([0-9A-Fa-f]{2})";
                Match match = Regex.Match(str, pattern_I2C);
                if (match.Success)
                {
                    string returnValue = match.Groups[1].Value;
                    // 由于DataReceived事件可能在一个非UI线程中触发，因此需要使用Invoke方法来更新UI
                    if (textBox5.InvokeRequired)
                    {
                        textBox5.Invoke(new Action(() =>
                        {
                            textBox5.Text = returnValue;
                        }));
                    }
                    else
                    {
                        textBox5.Text = returnValue;
                    }

                }
                else
                {

                }
                
                //byte[] str = new byte[serialPort1.BytesToRead];//接收字节数组
                //serialPort1.Read(str, 0, str.Length);//读取数据
                //textBox1.AppendText(Encoding.GetEncoding("GB2312").GetString(str));//显示数据
            }
            else//接收模式为十六进制模式
            {
                byte[] buffer = new byte[serialPort1.BytesToRead];//接收字节数组
                serialPort1.Read(buffer, 0, buffer.Length);//读取数据
                //byte data = (buffer[0]);//强制转换为byte类型
                //textBox1.AppendText("0X"+Convert.ToString(data,16)+" ");
                for(int i = 0; i < buffer.Length; i++)
                {
                    string data = Convert.ToString(buffer[i], 16).ToUpper();
                    textBox1.AppendText("0X"+(data.Length == 1 ? "0" + data : data)+" ");
                }
                textBox1.AppendText(Environment.NewLine);//换行
                //textBox1.AppendText(BitConverter.ToString(buffer));//显示数据

                //以下方法为教程内方法
                //byte data = (byte)serialPort1.ReadByte();//读取数据,并强制转换为byte类型
                //string str = Convert.ToString(data, 16).ToUpper();//将数据转换为十六进制并大写
                //textBox1.AppendText("0x"+(str.Length == 1 ? "0" + str : str)+" ");//显示数据
            }

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if(radioButton1.Checked)//发送模式为十六进制模式
            {
                UpdateTextBoxForHexInput(textBox2);//格式化输入
            }
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            UpdateTextBoxForHexInput(textBox3);//格式化输入
            if(textBox3.Text=="")
            {
                return;
            }
            byte address = Convert.ToByte(textBox3.Text, 16);//将十六进制字符串转换为字节数
            CheckBox[] checkBoxes = new CheckBox[8]
            {
                // 需要确保这些checkBox在Form的控件集合中，并且名称正确
                 checkBox17, checkBox18, checkBox19, checkBox20,
                 checkBox21, checkBox22, checkBox23, checkBox24
            };
            for (int i = 0; i < 8; i++)
            {
                checkBoxes[i].Checked = (address & (1 << i)) != 0;//根据字节的每一位设置复选框的选中状态
            }

        }


        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            UpdateTextBoxForHexInput(textBox4);//格式化输入
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            UpdateTextBoxForHexInput(textBox5);//格式化输入
        }
    }
}
