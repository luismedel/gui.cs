using Terminal.Gui;
using System;
using Mono.Terminal;
using System.Collections.Generic;

static class Demo {
	class Box10x : View {
		public Box10x (int x, int y) : base (new Rect (x, y, 10, 10))
		{
		}

		public override void Redraw (Rect region)
		{
			Driver.SetAttribute (ColorScheme.Focus);

			for (int y = 0; y < 10; y++) {
				Move (0, y);
				for (int x = 0; x < 10; x++) {

					Driver.AddRune ((Rune)('0' + (x + y) % 10));
				}
			}

		}
	}

	class Filler : View {
		public Filler (int w, int h) : base (w, h) { }
		public Filler (Rect rect) : base (rect)
		{
		}

		public override void Redraw (Rect region)
		{
			Driver.SetAttribute (ColorScheme.Focus);
			var f = Frame;

			for (int y = 0; y < f.Height; y++) {
				Move (0, y);
				for (int x = 0; x < f.Width; x++) {
					Rune r;
					switch (x % 3) {
					case 0:
						r = '.';
						break;
					case 1:
						r = 'o';
						break;
					default:
						r = 'O';
						break;
					}
					Driver.AddRune (r);
				}
			}
		}
	}


	static void ShowTextAlignments (View container)
	{
		container.Add (
			new Label (new Rect (0, 0, 40, 3), "1-Hello world, how are you doing today") { TextAlignment = TextAlignment.Left },
			new Label (new Rect (0, 4, 40, 3), "2-Hello world, how are you doing today") { TextAlignment = TextAlignment.Right },
			new Label (new Rect (0, 8, 40, 3), "3-Hello world, how are you doing today") { TextAlignment = TextAlignment.Centered },
			new Label (new Rect (0, 12, 40, 3), "4-Hello world, how are you doing today") { TextAlignment = TextAlignment.Justified });
	}

	static void ShowEntries (View container)
	{
		var scrollView = new ScrollView (new Rect (50, 10, 20, 8)) {
			ContentSize = new Size (100, 100),
			ContentOffset = new Point (-1, -1),
			ShowVerticalScrollIndicator = true,
			ShowHorizontalScrollIndicator = true
		};

		scrollView.Add (new Box10x (0, 0));
		//scrollView.Add (new Filler (new Rect (0, 0, 40, 40)));

		// This is just to debug the visuals of the scrollview when small
		var scrollView2 = new ScrollView (new Rect (72, 10, 3, 3)) {
			ContentSize = new Size (100, 100),
			ShowVerticalScrollIndicator = true,
			ShowHorizontalScrollIndicator = true
		};
		scrollView2.Add (new Box10x (0, 0));
		var progress = new ProgressBar (new Rect (68, 1, 10, 1));
		bool timer (MainLoop caller)
		{
			progress.Pulse ();
			return true;
		}

		Application.MainLoop.AddTimeout (TimeSpan.FromMilliseconds (300), timer);

		// Add some content
		container.Add (
			new Label (3, 6, "Login: "),
			new TextField (14, 6, 40, ""),
			new Label (3, 8, "Password: "),
			new TextField (14, 8, 40, "") { Secret = true },
			new FrameView (new Rect (3, 10, 25, 6), "Options"){
				new CheckBox (1, 0, "Remember me"),
				new RadioGroup (1, 2, new [] { "_Personal", "_Company" }),
			},
			new ListView (new Rect (60, 6, 16, 4), new string [] {
				"First row",
				"<>",
				"This is a very long row that should overflow what is shown",
				"4th",
				"There is an empty slot on the second row",
				"Whoa",
				"This is so cool"
			}),
			scrollView,
			//scrollView2,
			new Button (3, 19, "Ok"),
			new Button (10, 19, "Cancel"),
			progress,
			new Label (3, 22, "Press ESC and 9 to activate the menubar")
		);

	}

	public static Label ml2;

	static void NewFile ()
	{
		var d = new Dialog (
			"New File", 50, 20,
			new Button ("Ok", is_default: true) { Clicked = () => { Application.RequestStop (); } },
			new Button ("Cancel") { Clicked = () => { Application.RequestStop (); } });
		ml2 = new Label (1, 1, "Mouse Debug Line");
		d.Add (ml2);
		Application.Run (d);
	}

	// 
	// Creates a nested editor
	static void Editor (Toplevel top)
	{
		var tframe = top.Frame;
		var ntop = new Toplevel (tframe);
		var menu = new MenuBar (new MenuBarItem [] {
			new MenuBarItem ("_File", new MenuItem [] {
				new MenuItem ("_Close", "", () => {Application.RequestStop ();}),
			}),
			new MenuBarItem ("_Edit", new MenuItem [] {
				new MenuItem ("_Copy", "", null),
				new MenuItem ("C_ut", "", null),
				new MenuItem ("_Paste", "", null)
			}),
		});
		ntop.Add (menu);

		var win = new Window (new Rect (0, 1, tframe.Width, tframe.Height - 1), "/etc/passwd");
		ntop.Add (win);

		var text = new TextView (new Rect (0, 0, tframe.Width - 2, tframe.Height - 3));
		text.Text = System.IO.File.ReadAllText ("/etc/passwd");
		win.Add (text);

		Application.Run (ntop);
	}

	static bool Quit ()
	{
		var n = MessageBox.Query (50, 7, "Quit Demo", "Are you sure you want to quit this demo?", "Yes", "No");
		return n == 0;
	}

	static void Close ()
	{
		MessageBox.ErrorQuery (50, 5, "Error", "There is nothing to close", "Ok");
	}

	static void ShowLayout (View container)
	{
		View [,] views = new View [3, 2];
		View [,] fillers = new View [3, 2];
		int w = 25;
		int h = 10;
		int x, y;

		for (x = 0; x < 3; x++) {
			for (y = 0; y < 2; y++) {

				views [x, y] = new FrameView (new Rect (x * (w+1), y * (h+1), w, h), $"{x},{y}");
				fillers [x, y] = new Filler (3, 3);
			}
		}

		x = -1; y = 0;
		void Demo (Action<View, View> cback)
		{
			if (x < 2)
				x++;
			else {
				x = 0;
				y++;
			}
			if (y == 2)
				return;
			cback (views [x, y], fillers [x, y]);
			views [x, y].Add (fillers [x,y]);
			views [x, y].Layout ();
		}
		// Just add
		Demo ((host, filler) => { });
		Demo ((host, filler) => {
			filler.MarginLeft = 5;
		});
		Demo ((host, filler) => {
			filler.MarginRight = 5;
		});
		Demo ((host, filler) => {
			filler.AlignContent = AlignContent.Center;
			filler.Right = 1;
		});
		Demo ((host, filler) => {
			filler.AlignContent = AlignContent.Stretch;
		});
		Demo ((host, filler) => {
			filler.AlignContent = AlignContent.End;
		});

		foreach (var j in views){
			container.Add (j);
		}
	}
	// Watch what happens when I try to introduce a newline after the first open brace
	// it introduces a new brace instead, and does not indent.  Then watch me fight
	// the editor as more oddities happen.

	public static void Open ()
	{
		var d = new OpenDialog ("Open", "Open a file");
		Application.Run (d);
	}

	public static Label ml;
	static void Main ()
	{
		//Application.UseSystemConsole = true;
		Application.Init ();

		var top = Application.Top;
		var tframe = top.Frame;

		var win = new Window (new Rect (0, 1, tframe.Width, tframe.Height-1), "Hello");
		var menu = new MenuBar (new MenuBarItem [] {
			new MenuBarItem ("_File", new MenuItem [] {
				new MenuItem ("Text Editor Demo", "", () => { Editor (top); }),
				new MenuItem ("_New", "Creates new file", NewFile),
				new MenuItem ("_Open", "", Open),
				new MenuItem ("_Close", "", () => Close ()),
				new MenuItem ("_Quit", "", () => { if (Quit ()) top.Running = false; })
			}),
			new MenuBarItem ("_Edit", new MenuItem [] {
				new MenuItem ("_Copy", "", null),
				new MenuItem ("C_ut", "", null),
				new MenuItem ("_Paste", "", null)
			}),
		});

		int count = 0;

		ml = new Label (new Rect (3, 17, 47, 1), "Mouse: ");
		if (true) {
			ShowLayout (win);
		} else {
			ShowEntries (win);

			Application.RootMouseEvent += delegate (MouseEvent me) {

				ml.Text = $"Mouse: ({me.X},{me.Y}) - {me.Flags} {count++}";
			};

			win.Add (ml);
		}
		// ShowTextAlignments (win);
		top.Add (win);
		top.Add (menu);
		Application.Run ();
	}
}