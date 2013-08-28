﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Peek {
  public partial class fmMain : Form {
    string filepath = "";
    List<string> files = new List<string>();
    Image image = null;

    /// <summary>
    /// Constructor.
    /// </summary>
    public fmMain(string[] args) {
      if (args.Length > 0)
        for (int i = 0; i < args.Length; i++)
          this.filepath += args[i] + (i < (args.Length - 1) ? " " : "");

      InitializeComponent();
    }

    /// <summary>
    /// Implements event_KeyDown().
    /// </summary>
    private void fmMain_KeyDown(object sender, KeyEventArgs e) {
      switch (e.KeyCode) {
        case Keys.Left:
          this.selectPreviousFile();
          break;

        case Keys.Right:
          this.selectNextFile();
          break;

        case Keys.Delete:
          this.deleteFile();
          break;

        case Keys.W:
          if (e.Control) { Application.Exit(); }
          break;

        case Keys.Q:
        case Keys.Escape:
          Application.Exit();
          break;
      }
    }

    /// <summary>
    /// Implements event_Load().
    /// </summary>
    private void fmMain_Load(object sender, EventArgs e) {
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

      int screenHeight = 0;
      int screenLeft = 0;
      int screenTop = 0;
      int screenWidth = 0;

      foreach (Screen screen in Screen.AllScreens) {
        if (MousePosition.X >= screen.Bounds.Left &&
            MousePosition.X <= (screen.Bounds.Left + screen.Bounds.Width) &&
            MousePosition.Y >= screen.Bounds.Top &&
            MousePosition.Y <= (screen.Bounds.Top + screen.Bounds.Height)) {
              screenHeight = screen.Bounds.Height;
              screenLeft = screen.Bounds.Left;
              screenTop = screen.Bounds.Top;
              screenWidth = screen.Bounds.Width;

              break;
        }
      }

      this.Location = new Point(
        screenLeft,
        screenTop);

      this.Size = new Size(
        screenWidth,
        screenHeight);

      this.loadImageFromArguments();
    }

    /// <summary>
    /// Moves the selected file to the Recyclin Bin.
    /// </summary>
    private void deleteFile() {
      if (this.files.Count > 0) {
        int position = -1;

        for (int i = 0; i < this.files.Count; i++) {
          if (this.files[i] == this.filepath) {
            position = i;
            break;
          }
        }

        if (position > -1) {
          this.files.RemoveAt(position);

          this.pbImage.Visible = false;

          this.image.Dispose();
          this.image = null;

          try {
            FileIO.MoveToRecycleBin(this.filepath);
          }
          catch (Exception ex) {
            MessageBox.Show(ex.Message);
          }

          if (position == this.files.Count)
            position = 0;

          this.filepath = this.files[position];

          this.loadImageFromArguments();
        }
      }
    }

    /// <summary>
    /// Load and display the image stored in the global filepath.
    /// </summary>
    private void loadImageFromArguments() {
      if (string.IsNullOrWhiteSpace(this.filepath)) {
        Application.Exit();
        return;
      }

      if (this.files.Count == 0)
        this.scanForImageFiles();

      try {
        this.image = Image.FromFile(this.filepath);

        string filename = this.filepath.Substring(this.filepath.LastIndexOf(@"\") + 1);

        int imageHeight = image.Height;
        int imageWidth = image.Width;

        int setHeight = 0;
        int setLeft = 0;
        int setTop = 0;
        int setWidth = 0;

        if (imageHeight > this.ClientSize.Height ||
            imageWidth > this.ClientSize.Width) {
          if (imageHeight > this.ClientSize.Height &&
              imageWidth > this.ClientSize.Width) {
            decimal percentHeight = (((decimal)100 / (decimal)imageHeight) * (decimal)this.ClientSize.Height) / (decimal)100;
            decimal percentWidth = (((decimal)100 / (decimal)imageWidth) * (decimal)this.ClientSize.Width) / (decimal)100;

            if (percentWidth < percentHeight) {
              setHeight = (int)((decimal)imageHeight * percentWidth);
              setWidth = this.ClientSize.Width;
            }
            else {
              setHeight = this.ClientSize.Height;
              setWidth = (int)((decimal)imageWidth * percentHeight);
            }
          }
          else if (imageHeight > this.ClientSize.Height) {
            decimal percent = (((decimal)100 / (decimal)imageHeight) * (decimal)this.ClientSize.Height) / (decimal)100;

            setHeight = this.ClientSize.Height;
            setWidth = (int)((decimal)imageWidth * percent);
          }
          else {
            decimal percent = (((decimal)100 / (decimal)imageWidth) * (decimal)this.ClientSize.Width) / (decimal)100;

            setHeight = (int)((decimal)imageHeight * percent);
            setWidth = this.ClientSize.Width;
          }
        }
        else {
          setHeight = imageHeight;
          setWidth = imageWidth;
        }

        setLeft = (this.ClientSize.Width - setWidth) / 2;
        setTop = (this.ClientSize.Height - setHeight) / 2;

        this.pbImage.Visible = false;
        this.pbImage.Location = new Point(setLeft, setTop);
        this.pbImage.Size = new Size(setWidth, setHeight);
        this.pbImage.Image = this.image;
        this.pbImage.Visible = true;

        this.Text =
          filename +
          " (" + imageWidth.ToString() + "x" + imageHeight.ToString() + ")" +
          " - Peek";
      }
      catch (Exception ex) {
        MessageBox.Show(
          ex.Message,
          "Error",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }

    /// <summary>
    /// Scan the folder of the given file to make it possible to navigate the folder.
    /// </summary>
    private void scanForImageFiles() {
      string path = this.filepath.Substring(0, this.filepath.LastIndexOf(@"\"));

      if (Directory.Exists(path)) {
        List<string> extensions = new List<string>() { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp" };

        for (int i = 0; i < extensions.Count; i++) {
          string[] temp = Directory.GetFiles(path, extensions[i], SearchOption.TopDirectoryOnly);

          for (int j = 0; j < temp.Length; j++)
            this.files.Add(temp[j]);
        }

        this.files.Sort();
      }
    }

    /// <summary>
    /// Activate the next file in the folder.
    /// </summary>
    private void selectNextFile() {
      string temp = "";

      if (this.files.Count > 0) {
        for (int i = 0; i < this.files.Count; i++) {
          if (this.files[i] == this.filepath) {
            if (i == (this.files.Count - 1))
              temp = this.files[0];
            else
              temp = this.files[i + 1];

            break;
          }
        }
      }

      if (temp != "" &&
          temp != this.filepath) {
        this.filepath = temp;
        this.loadImageFromArguments();
      }
    }

    /// <summary>
    /// Activate the previous file in the folder.
    /// </summary>
    private void selectPreviousFile() {
      string temp = "";

      if (this.files.Count > 0) {
        for (int i = 0; i < this.files.Count; i++) {
          if (this.files[i] == this.filepath) {
            if (i == 0)
              temp = this.files[this.files.Count - 1];
            else
              temp = this.files[i - 1];

            break;
          }
        }
      }

      if (temp != "" &&
          temp != this.filepath) {
        this.filepath = temp;
        this.loadImageFromArguments();
      }
    }
  }
}
