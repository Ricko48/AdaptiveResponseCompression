namespace WFClientDemo;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        button1 = new Button();
        button2 = new Button();
        adaptiveCompressionLabel = new Label();
        standardCompressionLabel = new Label();
        fileSizeComboBox = new ComboBox();
        label9 = new Label();
        button3 = new Button();
        bandwidthLabel = new Label();
        SuspendLayout();
        // 
        // button1
        // 
        button1.Location = new Point(25, 164);
        button1.Margin = new Padding(3, 2, 3, 2);
        button1.Name = "button1";
        button1.Size = new Size(302, 22);
        button1.TabIndex = 0;
        button1.Text = "Download file using adaptive response compression";
        button1.UseVisualStyleBackColor = true;
        button1.Click += AdaptiveCompressionButtonClicked;
        // 
        // button2
        // 
        button2.Location = new Point(25, 256);
        button2.Margin = new Padding(3, 2, 3, 2);
        button2.Name = "button2";
        button2.Size = new Size(302, 22);
        button2.TabIndex = 1;
        button2.Text = "Download file using standard response compression";
        button2.UseVisualStyleBackColor = true;
        button2.Click += StandardCompressionButtonClicked;
        // 
        // adaptiveCompressionLabel
        // 
        adaptiveCompressionLabel.AutoSize = true;
        adaptiveCompressionLabel.Location = new Point(25, 200);
        adaptiveCompressionLabel.Name = "adaptiveCompressionLabel";
        adaptiveCompressionLabel.Size = new Size(124, 15);
        adaptiveCompressionLabel.TabIndex = 2;
        adaptiveCompressionLabel.Text = "Not downloaded yet...";
        // 
        // standardCompressionLabel
        // 
        standardCompressionLabel.AutoSize = true;
        standardCompressionLabel.Location = new Point(26, 291);
        standardCompressionLabel.Name = "standardCompressionLabel";
        standardCompressionLabel.Size = new Size(124, 15);
        standardCompressionLabel.TabIndex = 5;
        standardCompressionLabel.Text = "Not downloaded yet...";
        // 
        // fileSizeComboBox
        // 
        fileSizeComboBox.FormattingEnabled = true;
        fileSizeComboBox.Location = new Point(213, 106);
        fileSizeComboBox.Margin = new Padding(3, 2, 3, 2);
        fileSizeComboBox.Name = "fileSizeComboBox";
        fileSizeComboBox.Size = new Size(73, 23);
        fileSizeComboBox.TabIndex = 10;
        // 
        // label9
        // 
        label9.AutoSize = true;
        label9.Location = new Point(25, 109);
        label9.Name = "label9";
        label9.Size = new Size(182, 15);
        label9.TabIndex = 12;
        label9.Text = "Size of the file to be downloaded:";
        // 
        // button3
        // 
        button3.Location = new Point(25, 19);
        button3.Margin = new Padding(3, 2, 3, 2);
        button3.Name = "button3";
        button3.Size = new Size(200, 22);
        button3.TabIndex = 13;
        button3.Text = "Estimate download bandwidth";
        button3.UseVisualStyleBackColor = true;
        button3.Click += EstimateBandwidthButtonClicked;
        // 
        // bandwidthLabel
        // 
        bandwidthLabel.AutoSize = true;
        bandwidthLabel.Location = new Point(25, 56);
        bandwidthLabel.Name = "bandwidthLabel";
        bandwidthLabel.Size = new Size(110, 15);
        bandwidthLabel.TabIndex = 14;
        bandwidthLabel.Text = "Not estimated yet...";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(395, 329);
        Controls.Add(bandwidthLabel);
        Controls.Add(button3);
        Controls.Add(label9);
        Controls.Add(fileSizeComboBox);
        Controls.Add(standardCompressionLabel);
        Controls.Add(adaptiveCompressionLabel);
        Controls.Add(button2);
        Controls.Add(button1);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Margin = new Padding(3, 2, 3, 2);
        Name = "Form1";
        Text = "Form1";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button button1;
    private Button button2;
    private Label adaptiveCompressionLabel;
    private Label standardCompressionLabel;
    private ComboBox fileSizeComboBox;
    private Label label9;
    private Button button3;
    private Label bandwidthLabel;
}
