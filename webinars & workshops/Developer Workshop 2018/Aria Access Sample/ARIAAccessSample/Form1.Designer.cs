namespace DevWorkshop2018AriaAcess
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSearchPatient = new System.Windows.Forms.Button();
            this.btnCreatePatient = new System.Windows.Forms.Button();
            this.btnCreateAppointment = new System.Windows.Forms.Button();
            this.txtPatientResponse = new System.Windows.Forms.RichTextBox();
            this.txtCreatePatientFilePath = new System.Windows.Forms.TextBox();
            this.lblCreatePatient = new System.Windows.Forms.Label();
            this.lblCreateAppointment = new System.Windows.Forms.Label();
            this.SearchPatient = new System.Windows.Forms.Panel();
            this.btnfhirSearchPatient = new System.Windows.Forms.Button();
            this.lbFirstName = new System.Windows.Forms.Label();
            this.txtFirstName = new System.Windows.Forms.TextBox();
            this.lblLastName = new System.Windows.Forms.Label();
            this.txtLastName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtApptResponse = new System.Windows.Forms.RichTextBox();
            this.btnSearchAppointment = new System.Windows.Forms.Button();
            this.lblHospital = new System.Windows.Forms.Label();
            this.txtHospital = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lblSearchAppt = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtCreatePatientResp = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtCreateApptFilePath = new System.Windows.Forms.TextBox();
            this.txtCreateApptResp = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtIdentityToken = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtAccessToken = new System.Windows.Forms.TextBox();
            this.txtDepartment = new System.Windows.Forms.TextBox();
            this.lblDepartment = new System.Windows.Forms.Label();
            this.txtMachineId = new System.Windows.Forms.TextBox();
            this.lblMachineId = new System.Windows.Forms.Label();
            this.SearchPatient.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSearchPatient
            // 
            this.btnSearchPatient.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearchPatient.Location = new System.Drawing.Point(226, 14);
            this.btnSearchPatient.Name = "btnSearchPatient";
            this.btnSearchPatient.Size = new System.Drawing.Size(128, 39);
            this.btnSearchPatient.TabIndex = 0;
            this.btnSearchPatient.Text = "Search Patient";
            this.btnSearchPatient.UseVisualStyleBackColor = true;
            this.btnSearchPatient.Click += new System.EventHandler(this.btnSearchPatient_Click);
            // 
            // btnCreatePatient
            // 
            this.btnCreatePatient.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreatePatient.Location = new System.Drawing.Point(0, 73);
            this.btnCreatePatient.Name = "btnCreatePatient";
            this.btnCreatePatient.Size = new System.Drawing.Size(128, 39);
            this.btnCreatePatient.TabIndex = 1;
            this.btnCreatePatient.Text = "Create Patient";
            this.btnCreatePatient.UseVisualStyleBackColor = true;
            this.btnCreatePatient.Click += new System.EventHandler(this.btnCreatePatient_Click);
            // 
            // btnCreateAppointment
            // 
            this.btnCreateAppointment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreateAppointment.Location = new System.Drawing.Point(16, 73);
            this.btnCreateAppointment.Name = "btnCreateAppointment";
            this.btnCreateAppointment.Size = new System.Drawing.Size(128, 39);
            this.btnCreateAppointment.TabIndex = 3;
            this.btnCreateAppointment.Text = "Create Appointment";
            this.btnCreateAppointment.UseVisualStyleBackColor = true;
            this.btnCreateAppointment.Click += new System.EventHandler(this.btnCreateAppointment_Click);
            // 
            // txtPatientResponse
            // 
            this.txtPatientResponse.Location = new System.Drawing.Point(3, 79);
            this.txtPatientResponse.Name = "txtPatientResponse";
            this.txtPatientResponse.Size = new System.Drawing.Size(428, 319);
            this.txtPatientResponse.TabIndex = 5;
            this.txtPatientResponse.Text = "";
            // 
            // txtCreatePatientFilePath
            // 
            this.txtCreatePatientFilePath.Location = new System.Drawing.Point(3, 29);
            this.txtCreatePatientFilePath.Multiline = true;
            this.txtCreatePatientFilePath.Name = "txtCreatePatientFilePath";
            this.txtCreatePatientFilePath.Size = new System.Drawing.Size(428, 38);
            this.txtCreatePatientFilePath.TabIndex = 13;
            this.txtCreatePatientFilePath.Text = "C:\\Requests\\createpatient.json";
            this.txtCreatePatientFilePath.TextChanged += new System.EventHandler(this.txtCreatePatientFilePath_TextChanged);
            // 
            // lblCreatePatient
            // 
            this.lblCreatePatient.AutoSize = true;
            this.lblCreatePatient.Location = new System.Drawing.Point(3, 13);
            this.lblCreatePatient.Name = "lblCreatePatient";
            this.lblCreatePatient.Size = new System.Drawing.Size(136, 13);
            this.lblCreatePatient.TabIndex = 15;
            this.lblCreatePatient.Text = "Create Patient Request File";
            // 
            // lblCreateAppointment
            // 
            this.lblCreateAppointment.AutoSize = true;
            this.lblCreateAppointment.Location = new System.Drawing.Point(3, 10);
            this.lblCreateAppointment.Name = "lblCreateAppointment";
            this.lblCreateAppointment.Size = new System.Drawing.Size(162, 13);
            this.lblCreateAppointment.TabIndex = 16;
            this.lblCreateAppointment.Text = "Create Appointment Request File";
            // 
            // SearchPatient
            // 
            this.SearchPatient.Controls.Add(this.btnfhirSearchPatient);
            this.SearchPatient.Controls.Add(this.lbFirstName);
            this.SearchPatient.Controls.Add(this.txtFirstName);
            this.SearchPatient.Controls.Add(this.lblLastName);
            this.SearchPatient.Controls.Add(this.txtLastName);
            this.SearchPatient.Controls.Add(this.btnSearchPatient);
            this.SearchPatient.Controls.Add(this.txtPatientResponse);
            this.SearchPatient.Location = new System.Drawing.Point(26, 26);
            this.SearchPatient.Name = "SearchPatient";
            this.SearchPatient.Size = new System.Drawing.Size(516, 421);
            this.SearchPatient.TabIndex = 17;
            // 
            // btnfhirSearchPatient
            // 
            this.btnfhirSearchPatient.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnfhirSearchPatient.Location = new System.Drawing.Point(370, 17);
            this.btnfhirSearchPatient.Name = "btnfhirSearchPatient";
            this.btnfhirSearchPatient.Size = new System.Drawing.Size(128, 39);
            this.btnfhirSearchPatient.TabIndex = 13;
            this.btnfhirSearchPatient.Text = "FHIR Search Patient";
            this.btnfhirSearchPatient.UseVisualStyleBackColor = true;
            this.btnfhirSearchPatient.Click += new System.EventHandler(this.btnfhirSearchPatient_Click);
            // 
            // lbFirstName
            // 
            this.lbFirstName.AutoSize = true;
            this.lbFirstName.Location = new System.Drawing.Point(20, 53);
            this.lbFirstName.Name = "lbFirstName";
            this.lbFirstName.Size = new System.Drawing.Size(54, 13);
            this.lbFirstName.TabIndex = 12;
            this.lbFirstName.Text = "FirstName";
            // 
            // txtFirstName
            // 
            this.txtFirstName.Location = new System.Drawing.Point(96, 50);
            this.txtFirstName.Name = "txtFirstName";
            this.txtFirstName.Size = new System.Drawing.Size(124, 20);
            this.txtFirstName.TabIndex = 11;
            // 
            // lblLastName
            // 
            this.lblLastName.AutoSize = true;
            this.lblLastName.Location = new System.Drawing.Point(19, 17);
            this.lblLastName.Name = "lblLastName";
            this.lblLastName.Size = new System.Drawing.Size(55, 13);
            this.lblLastName.TabIndex = 10;
            this.lblLastName.Text = "LastName";
            // 
            // txtLastName
            // 
            this.txtLastName.Location = new System.Drawing.Point(96, 14);
            this.txtLastName.Name = "txtLastName";
            this.txtLastName.Size = new System.Drawing.Size(124, 20);
            this.txtLastName.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblMachineId);
            this.panel1.Controls.Add(this.txtMachineId);
            this.panel1.Controls.Add(this.lblDepartment);
            this.panel1.Controls.Add(this.txtDepartment);
            this.panel1.Controls.Add(this.txtApptResponse);
            this.panel1.Controls.Add(this.btnSearchAppointment);
            this.panel1.Controls.Add(this.lblHospital);
            this.panel1.Controls.Add(this.txtHospital);
            this.panel1.Location = new System.Drawing.Point(579, 26);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(415, 421);
            this.panel1.TabIndex = 18;
            // 
            // txtApptResponse
            // 
            this.txtApptResponse.Location = new System.Drawing.Point(16, 105);
            this.txtApptResponse.Name = "txtApptResponse";
            this.txtApptResponse.Size = new System.Drawing.Size(386, 293);
            this.txtApptResponse.TabIndex = 15;
            this.txtApptResponse.Text = "";
            // 
            // btnSearchAppointment
            // 
            this.btnSearchAppointment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearchAppointment.Location = new System.Drawing.Point(236, 14);
            this.btnSearchAppointment.Name = "btnSearchAppointment";
            this.btnSearchAppointment.Size = new System.Drawing.Size(128, 39);
            this.btnSearchAppointment.TabIndex = 14;
            this.btnSearchAppointment.Text = "Search Appointment";
            this.btnSearchAppointment.UseVisualStyleBackColor = true;
            this.btnSearchAppointment.Click += new System.EventHandler(this.btnSearchAppointment_Click);
            // 
            // lblHospital
            // 
            this.lblHospital.AutoSize = true;
            this.lblHospital.Location = new System.Drawing.Point(3, 14);
            this.lblHospital.Name = "lblHospital";
            this.lblHospital.Size = new System.Drawing.Size(45, 13);
            this.lblHospital.TabIndex = 13;
            this.lblHospital.Text = "Hospital";
            this.lblHospital.Click += new System.EventHandler(this.lblMachineId_Click);
            // 
            // txtHospital
            // 
            this.txtHospital.Location = new System.Drawing.Point(81, 14);
            this.txtHospital.Name = "txtHospital";
            this.txtHospital.Size = new System.Drawing.Size(129, 20);
            this.txtHospital.TabIndex = 12;
            this.txtHospital.Text = "Varian";
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSearch.Location = new System.Drawing.Point(23, 6);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(115, 17);
            this.lblSearch.TabIndex = 19;
            this.lblSearch.Text = "Search Patient";
            // 
            // lblSearchAppt
            // 
            this.lblSearchAppt.AutoSize = true;
            this.lblSearchAppt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSearchAppt.Location = new System.Drawing.Point(585, 6);
            this.lblSearchAppt.Name = "lblSearchAppt";
            this.lblSearchAppt.Size = new System.Drawing.Size(154, 17);
            this.lblSearchAppt.TabIndex = 20;
            this.lblSearchAppt.Text = "Search Appointment";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.txtCreatePatientResp);
            this.panel2.Controls.Add(this.btnCreatePatient);
            this.panel2.Controls.Add(this.lblCreatePatient);
            this.panel2.Controls.Add(this.txtCreatePatientFilePath);
            this.panel2.Location = new System.Drawing.Point(26, 487);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(450, 280);
            this.panel2.TabIndex = 21;
            // 
            // txtCreatePatientResp
            // 
            this.txtCreatePatientResp.Location = new System.Drawing.Point(6, 118);
            this.txtCreatePatientResp.Multiline = true;
            this.txtCreatePatientResp.Name = "txtCreatePatientResp";
            this.txtCreatePatientResp.Size = new System.Drawing.Size(425, 144);
            this.txtCreatePatientResp.TabIndex = 18;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.txtCreateApptFilePath);
            this.panel3.Controls.Add(this.txtCreateApptResp);
            this.panel3.Controls.Add(this.btnCreateAppointment);
            this.panel3.Controls.Add(this.lblCreateAppointment);
            this.panel3.Location = new System.Drawing.Point(588, 487);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(406, 280);
            this.panel3.TabIndex = 22;
            // 
            // txtCreateApptFilePath
            // 
            this.txtCreateApptFilePath.Location = new System.Drawing.Point(0, 26);
            this.txtCreateApptFilePath.Multiline = true;
            this.txtCreateApptFilePath.Name = "txtCreateApptFilePath";
            this.txtCreateApptFilePath.Size = new System.Drawing.Size(393, 41);
            this.txtCreateApptFilePath.TabIndex = 18;
            this.txtCreateApptFilePath.Text = "C:\\Requests\\createAppointment.json";
            this.txtCreateApptFilePath.TextChanged += new System.EventHandler(this.txtCreateApptFilePath_TextChanged);
            // 
            // txtCreateApptResp
            // 
            this.txtCreateApptResp.Location = new System.Drawing.Point(16, 133);
            this.txtCreateApptResp.Multiline = true;
            this.txtCreateApptResp.Name = "txtCreateApptResp";
            this.txtCreateApptResp.Size = new System.Drawing.Size(377, 144);
            this.txtCreateApptResp.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(23, 467);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 17);
            this.label1.TabIndex = 23;
            this.label1.Text = "Create Patient";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(591, 467);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 17);
            this.label2.TabIndex = 24;
            this.label2.Text = "Create Appointment";
            // 
            // txtIdentityToken
            // 
            this.txtIdentityToken.Location = new System.Drawing.Point(26, 798);
            this.txtIdentityToken.Multiline = true;
            this.txtIdentityToken.Name = "txtIdentityToken";
            this.txtIdentityToken.ReadOnly = true;
            this.txtIdentityToken.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtIdentityToken.Size = new System.Drawing.Size(431, 122);
            this.txtIdentityToken.TabIndex = 25;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(23, 779);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Identity Token";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(592, 779);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 17);
            this.label4.TabIndex = 26;
            this.label4.Text = "Access Token";
            // 
            // txtAccessToken
            // 
            this.txtAccessToken.Location = new System.Drawing.Point(595, 799);
            this.txtAccessToken.Multiline = true;
            this.txtAccessToken.Name = "txtAccessToken";
            this.txtAccessToken.ReadOnly = true;
            this.txtAccessToken.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAccessToken.Size = new System.Drawing.Size(386, 121);
            this.txtAccessToken.TabIndex = 27;
            // 
            // txtDepartment
            // 
            this.txtDepartment.Location = new System.Drawing.Point(81, 40);
            this.txtDepartment.Name = "txtDepartment";
            this.txtDepartment.Size = new System.Drawing.Size(129, 20);
            this.txtDepartment.TabIndex = 16;
            this.txtDepartment.Text = "Developers";
            // 
            // lblDepartment
            // 
            this.lblDepartment.AutoSize = true;
            this.lblDepartment.Location = new System.Drawing.Point(3, 40);
            this.lblDepartment.Name = "lblDepartment";
            this.lblDepartment.Size = new System.Drawing.Size(62, 13);
            this.lblDepartment.TabIndex = 17;
            this.lblDepartment.Text = "Department";
            // 
            // txtMachineId
            // 
            this.txtMachineId.Location = new System.Drawing.Point(81, 66);
            this.txtMachineId.Name = "txtMachineId";
            this.txtMachineId.Size = new System.Drawing.Size(129, 20);
            this.txtMachineId.TabIndex = 18;
            this.txtMachineId.Text = "Def_CTScanner";
            // 
            // lblMachineId
            // 
            this.lblMachineId.AutoSize = true;
            this.lblMachineId.Location = new System.Drawing.Point(3, 66);
            this.lblMachineId.Name = "lblMachineId";
            this.lblMachineId.Size = new System.Drawing.Size(60, 13);
            this.lblMachineId.TabIndex = 19;
            this.lblMachineId.Text = "Machine Id";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1158, 932);
            this.Controls.Add(this.txtAccessToken);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtIdentityToken);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.lblSearchAppt);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.SearchPatient);
            this.Name = "Form1";
            this.Text = "Form1";
            this.SearchPatient.ResumeLayout(false);
            this.SearchPatient.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSearchPatient;
        private System.Windows.Forms.Button btnCreatePatient;
        private System.Windows.Forms.Button btnCreateAppointment;
        private System.Windows.Forms.RichTextBox txtPatientResponse;
        private System.Windows.Forms.TextBox txtCreatePatientFilePath;
        private System.Windows.Forms.Label lblCreatePatient;
        private System.Windows.Forms.Label lblCreateAppointment;
        private System.Windows.Forms.Panel SearchPatient;
        private System.Windows.Forms.Label lbFirstName;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.Label lblLastName;
        private System.Windows.Forms.TextBox txtLastName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox txtApptResponse;
        private System.Windows.Forms.Button btnSearchAppointment;
        private System.Windows.Forms.Label lblHospital;
        private System.Windows.Forms.TextBox txtHospital;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Label lblSearchAppt;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCreatePatientResp;
        private System.Windows.Forms.TextBox txtCreateApptResp;
        private System.Windows.Forms.TextBox txtCreateApptFilePath;
        private System.Windows.Forms.TextBox txtIdentityToken;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtAccessToken;
        private System.Windows.Forms.Button btnfhirSearchPatient;
        private System.Windows.Forms.Label lblMachineId;
        private System.Windows.Forms.TextBox txtMachineId;
        private System.Windows.Forms.Label lblDepartment;
        private System.Windows.Forms.TextBox txtDepartment;
    }
}

