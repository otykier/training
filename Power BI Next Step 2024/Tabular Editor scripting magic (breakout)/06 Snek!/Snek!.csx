#r "System.Drawing"

// PLAY SNEK!

// To use this script:
// 1. Open it in Tabular Editor's Script Window
// 2. Run it and have fun

// Created by Kurt Buhler @ data-goblins.com
// Modified by Daniel Otykier to add a 'Snek!' calc table to the model, which gets its
// DAX expression updated continuously as Snek! noms the bugs. The calc table has a
// few measures as well, so high scores can be visualized in client tools.

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Net;
using System.IO;

ScriptHelper.WaitFormVisible = false;
Application.UseWaitCursor = false;


class SnekGame : Form
{
    private List<Point> _snek = new List<Point>();
    private List<Color> _tailColors = new List<Color>();
    private Point _food;
    private Point _direction;
    private Timer _timer = new Timer();
    private bool _gameOver = false;
    private int _blockSize = 10;
    CalculatedTable scoreTable = null;
    
    private Brush _foodColor = Brushes.Red;
    private Color foodColor;


    private Image _backgroundImage;
    private float _backgroundImageOpacity = 0.2f;
    private readonly Model model;

    private int snekLength = 0;
    int highScore;
    
    public SnekGame(Model model)
    {
        this.model = model;
        this.highScore = Convert.ToInt32(model.GetAnnotation("DataGoblins_SnekHighScore"));
    }

    private void SnekGame_FormClosing(object sender, FormClosingEventArgs e)
    {
    _gameOver = true;
    }

    public class DifficultyDialog : Form
    {
    private ComboBox _comboBox;
    private Button _button;

    public DifficultyDialog()
    {
        Text = "There's a bug in the model! Release the Snek! 🐍";
        Font = new Font("Futura PT Medium", 14);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Width = 350;
        Height = 150;

        _comboBox = new ComboBox();
        _comboBox.Items.Add("Easy");
        _comboBox.Items.Add("Medium");
        _comboBox.Items.Add("Hard");
        _comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _comboBox.SelectedIndex = 1;
        _comboBox.Location = new System.Drawing.Point(20, 20);
        _comboBox.Width = 120;
        _comboBox.Height = 30;

        _button = new Button();
        _button.Text = "GO SNEK GO!";
        _button.DialogResult = DialogResult.OK;
        _button.Location = new System.Drawing.Point(160, 20);
        _button.Width = 150;
        _button.Height = 30;

        Controls.Add(_comboBox);
        Controls.Add(_button);
    }

    public string Difficulty
    {
        get { return (string)_comboBox.SelectedItem; }
    }
    }
    public void SnekGame_Load(object sender, EventArgs e)
    {
    using (var dialog = new DifficultyDialog())
    {
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            switch (dialog.Difficulty)
            {
                case "Easy":
                    _timer.Interval = 200;
                    break;
                case "Medium":
                    _timer.Interval = 125;
                    break;
                case "Hard":
                    _timer.Interval = 75;
                    break;
            }
        }
        else
        {
            Close();
            return;
        }
    }
        scoreTable = model.Tables.FindByName("Snek!") as CalculatedTable;
        if(scoreTable == null) {
            scoreTable = model.AddCalculatedTable("Snek!");
            scoreTable.Expression = "DATATABLE(\"Player\", STRING, \"GameStarted\", DATETIME, \"Time\", DATETIME, \"Action\", STRING, \"Score\", INTEGER, {\r\n  {,,,,}\r\n})";
            scoreTable.AddCalculatedTableColumn("Player", "Player", dataType: DataType.String);
            scoreTable.AddCalculatedTableColumn("GameStarted", "GameStarted", dataType: DataType.DateTime);
            scoreTable.AddCalculatedTableColumn("Time", "Time", dataType: DataType.DateTime).IsHidden = true;
            scoreTable.AddCalculatedTableColumn("Action", "Action", dataType: DataType.String).IsHidden = true;
            scoreTable.AddCalculatedTableColumn("Score", "Score", dataType: DataType.Int64);
            scoreTable.AddMeasure("High score", "MAX('Snek!'[Score])");
            scoreTable.AddMeasure("Best player", "SELECTCOLUMNS(TOPN(1, SUMMARIZE('Snek!', 'Snek!'[Player], \"s\", CALCULATE(MAX('Snek!'[Score]))), [s], DESC), \"Player\", [Player])");
            scoreTable.AddMeasure("Longest play (seconds)", "MAXX(SUMMARIZE('Snek!', 'Snek!'[Player], 'Snek!'[GameStarted]), DATEDIFF(MIN('Snek!'[Time]), MAX('Snek!'[Time]), SECOND))");
        }
        _snek.Add(new Point(10, 10));
        _snek.Add(new Point(9, 10));
        _snek.Add(new Point(8, 10));
        _direction = new Point(1, 0);
        _food = GetRandomLocation();

        _timer.Tick += Timer_Tick;
        _timer.Start();
        gameStarted = DateTime.Now;
        AddRecord("Game start");

        Width = 300;
        Height = 300;
        StartPosition = FormStartPosition.CenterScreen;
 
        using (var webClient = new WebClient())
        {
            var imageBytes = webClient.DownloadData("https://images.squarespace-cdn.com/content/v1/5c3a30e0f7939271ddaa7e4f/edac6702-fde0-407c-80e1-5591b0433770/DataGoblins_Lighter_NoSubtitle.png");
            _backgroundImage = Image.FromStream(new MemoryStream(imageBytes));
        }

        Paint += SnekGame_Paint;
        FormClosing += SnekGame_FormClosing;
    }
    
    DateTime gameStarted;
    
    private void AddRecord(string action)
    {
        var record = "{" + string.Format("\"{0}\", dt\"{1}\", dt\"{2}\", \"{3}\", {4}", Environment.UserName, gameStarted.ToString("yyyy-MM-dd hh:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), action, snekLength) + "}";
        scoreTable.Expression =
            scoreTable.Expression.Replace("\r\n})", ",\r\n  " + record + "\r\n})");
    }

    private void SnekGame_Paint(object sender, PaintEventArgs e)
    {
        if (_backgroundImage != null)
        {
            float imageOpacity = 0.2f;
            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = imageOpacity;
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
    
            e.Graphics.DrawImage(
                _backgroundImage,
                new Rectangle(0, 0, 285, 220),
                0, 0, _backgroundImage.Width, _backgroundImage.Height,
                GraphicsUnit.Pixel,
                attributes
            );
        }
    
    
        for (int i = 0; i < _snek.Count; i++)
        {
            Brush color;
            if (i == 0)
            {
                color = new SolidBrush(ColorTranslator.FromHtml("#98a27c"));
            }
            else if (i <= _tailColors.Count)
            {
                color = new SolidBrush(_tailColors[i - 1]);
            }
            else
            {
                color = Brushes.Black;
            }
            DrawBlock(_food, _foodColor, e.Graphics);
            DrawBlock(_snek[i], color, e.Graphics);
        }

        
    
        if (_gameOver)
        {
            e.Graphics.DrawString("Game over", new Font("Arial", 16), Brushes.Black, new Point(50, 100));
        }
        else
        {
            DrawLength(e.Graphics);
        }
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x02000000;
            return cp;
        }
    }

    private Point GetRandomLocation()
    {
        Random random = new Random();
        int x = random.Next(0, (Width / _blockSize - 5) - 1);
        int y = random.Next(0, (Height / _blockSize - 5) - 1);
        return new Point(x, y);
    }

    private void DrawBlock(Point location, Brush color, Graphics g)
        {
            var brush = new SolidBrush(Color.Black);
            var pen = new Pen(brush, 2);
            var rect = new Rectangle(location.X * _blockSize, location.Y * _blockSize, _blockSize, _blockSize);
            g.FillRectangle(_foodColor, rect);
            g.DrawRectangle(pen, rect);
        }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (_gameOver) return;
    
        Point next = new Point(_snek[0].X + _direction.X, _snek[0].Y + _direction.Y);
    
        if (next.X < 0 || next.X >= Width / 10.7 || next.Y < 0 || next.Y >= Height / 11.6 || _snek.Contains(next))
        {
            _gameOver = true;
            System.Media.SystemSounds.Beep.Play();
            AddRecord("Game end");
            model.SetAnnotation("DataGoblins_SnekHighScore", Convert.ToString(highScore));
            Error( "Game over! You scored: " + snekLength + "\n\nHigh Score: " + highScore );
            DialogResult = DialogResult.OK;
            return;
        }
    
        _snek.Insert(0, next);
    
        if (next == _food)
        {
            System.Media.SystemSounds.Beep.Play();
            _foodColor = new SolidBrush(GetRandomColor());
            _food = GetRandomLocation();
            snekLength++;
            AddRecord("Nom!");
        
            if (snekLength > highScore)
            {
                highScore = snekLength;
            }
        
            _tailColors.Add(((_foodColor as SolidBrush).Color));
        }
        else
        {
            _snek.RemoveAt(_snek.Count - 1);
        }
    
        Invalidate();
    }

    private Color GetRandomColor()
    {
        Random rnd = new Random();
        KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
        KnownColor randomColorName = names[rnd.Next(names.Length)];
        Color randomColor = Color.FromKnownColor(randomColorName);
    
        while (randomColor.A == 0)
        {
            randomColorName = names[rnd.Next(names.Length)];
            randomColor = Color.FromKnownColor(randomColorName);
        }
    
        return randomColor;
    }

    private void DrawLength(Graphics graphics)
    {
        Font drawFont = new Font("Futura PT Medium", 12);
        Brush drawBrush = new SolidBrush(Color.Black);
        graphics.DrawString(string.Format("Score: {0}", snekLength), drawFont, drawBrush, new PointF(10, 10));
        graphics.DrawString(string.Format("High Score: {0}", highScore), drawFont, drawBrush, new PointF(120, 10));
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Up:
                _direction = new Point(0, -1);
                break;
            case Keys.Down:
                _direction = new Point(0, 1);
                break;
            case Keys.Left:
                _direction = new Point(-1, 0);
                break;
            case Keys.Right:
                _direction = new Point(1, 0);
                break;
            case Keys.Cancel:
                Close();
                break;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}

SnekGame game = new SnekGame(Model);
game.StartPosition = FormStartPosition.CenterScreen;
game.Load += game.SnekGame_Load; 
game.TopMost = true;
System.Threading.Tasks.Task.Run(() => {
    game.ShowDialog();
});
