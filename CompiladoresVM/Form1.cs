﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompiladoresVM
{
    public partial class Form1 : Form
    {       
        VMCore vm;
        bool stepByStep = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                vm = new VMCore(2000, 2000);

                vm.ParseFromFile(openFileDialog1.FileName);

                txtInput.Text = "";
                txtOutput.Text = "";

                listViewMemoryProgram.Items.Clear();
                for (int i = 0; i < vm.instructionCount; i++)
                {
                    var ins = vm.instructions[i];
                    listViewMemoryProgram.Items.Add(new ListViewItem(new string[] { i.ToString(), ins.label, ins.opcode, ins.arg1, ins.arg2, ins.comment, ins.useLabel.ToString(), }));

                    if (ins.useLabel)
                    {
                        listViewMemoryProgram.Items[i].ForeColor = Color.Blue;
                    }
                    else
                    {
                        listViewMemoryProgram.Items[i].ForeColor = Color.Black;
                    }

                    if (ins.isLabel)
                    {
                        listViewMemoryProgram.Items[i].ForeColor = Color.DarkGray;
                    }
                }

                ProccessIO();
            }
        }

        private void adicionarBreakpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewMemoryProgram.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewMemoryProgram.SelectedItems)
                {
                    vm.breakpoints.Add(listViewMemoryProgram.Items.IndexOf(item));
                    item.BackColor = Color.LightSalmon;
                }
            }
        }

        private void removerBreakpointToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listViewMemoryProgram.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewMemoryProgram.SelectedItems)
                {
                    int idx = vm.breakpoints.IndexOf(listViewMemoryProgram.Items.IndexOf(item));
                    if (idx != -1)
                        vm.breakpoints.RemoveAt(idx);
                    item.BackColor = Color.White;
                }
            }
        }

        private void UpdateInterface()
        {
            listViewMemoryData.Items.Clear();
            for (int i = 0; i <= vm.S; i++)
            {
                var m = vm.M[i];
                listViewMemoryData.Items.Add(new ListViewItem(new string[] { i.ToString(), m.ToString(), }));
            }

            labelI.Text = "I: " + vm.I.ToString();
            labelS.Text = "S: " + vm.S.ToString();

            listViewMemoryProgram.SelectedItems.Clear();
            //listViewMemoryProgram.Items[vm.I - 1].Selected = true;
            listViewMemoryProgram.Items[vm.I].Selected = true;
            //listViewMemoryProgram.Items[vm.I].BackColor = Color.Green;
            listViewMemoryProgram.EnsureVisible(vm.I - 1);

            if (vm.S >= 0)
            {
                //listViewMemoryData.SelectedItems.Clear();
                //listViewMemoryData.Items[vm.S].Selected = true;
                listViewMemoryData.Items[vm.S].BackColor = Color.LightBlue;
                //listViewMemoryData.EnsureVisible(vm.S);
                //listViewMemoryData.Select();
            }

            foreach (ListViewItem item in listViewMemoryProgram.Items)
            {
                item.BackColor = Color.White;
            }
            foreach (int I in vm.breakpoints)
            {
                listViewMemoryProgram.Items[I].BackColor = Color.LightSalmon;
            }
        }

        private void ProccessIO()
        {
            txtInput.Enabled = vm.io.waitingInput;
            txtOutput.Enabled = vm.io.waitingOutput;
            groupBoxInput.BackColor = vm.io.waitingInput ? Color.LightGreen : Color.LightSalmon;

            if (txtInput.Enabled)
            {
                txtInput.Focus();
                txtInput.Select(0, 0);
            }

            int parseResult = 0;

            if (vm.io.waitingOutput)
            {
                int ioReadData = vm.IORead();

                txtOutput.Text += ioReadData.ToString() + "\r\n";
            }

            if (vm.io.waitingInput && int.TryParse(txtInput.Text, out parseResult))
            {
                int ioWriteData = int.Parse(txtInput.Text);
                vm.IOWrite(ioWriteData);

                txtInput.Text = "";
            }

            txtInput.Enabled = vm.io.waitingInput;
            txtOutput.Enabled = true;
            groupBoxInput.BackColor = vm.io.waitingInput ? Color.LightGreen : Color.LightSalmon;

            txtOutput.Select(txtOutput.TextLength, txtOutput.TextLength);
            txtOutput.ScrollToCaret();

            if (!txtInput.Enabled)
                listViewMemoryProgram.Select();
        }

        bool VMCycle()
        {
            ProccessIO();
            bool continueExec = vm.SingleStep();
            ProccessIO();

            return continueExec;
        }

        void StepByStepExec()
        {
            stepByStep = true;
            VMCycle();
            UpdateInterface();
        }

        void FullRun()
        {
            stepByStep = false;
            while (VMCycle()) ;
            UpdateInterface();
        }

        private void continuarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StepByStepExec();
        }

        private void executarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FullRun();
        }

        private void listViewMemoryProgram_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (stepByStep)
                    StepByStepExec();
                else
                    FullRun();

                e.SuppressKeyPress = true;
            }
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (stepByStep)
                    StepByStepExec();
                else
                    FullRun();

                e.SuppressKeyPress = true;
            }
        }

        private void listViewMemoryProgram_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
