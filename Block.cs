using System;
using System.Drawing;

namespace MySweeper {
    class Block : IDisposable {
        #region Static members
        public static void LoadResources(Image icons) {
            number_icons_ = new Image[8];

            for (var i = 0; i < 8; i++) {
                var x = i * 16;
                var result = new Bitmap(16, 16);
                var g = Graphics.FromImage(result);
                g.Clear(Color.Transparent);
                g.DrawImage(icons, new Rectangle(0, 0, 16, 16), x, 0, 16, 16, GraphicsUnit.Pixel);
                g.Dispose();

                number_icons_[i] = result;
            }

            status_images_ = new Image[5];

            for (var i = 0; i < 5; i++) {
                var x = i * 16;
                var result = new Bitmap(16, 16);
                var g = Graphics.FromImage(result);
                g.Clear(Color.Transparent);
                g.DrawImage(icons, new Rectangle(0, 0, 16, 16), x, 48, 16, 16, GraphicsUnit.Pixel);
                g.Dispose();

                status_images_[i] = result;
            }

            block_cover_brush_ = new SolidBrush(Color.Gray);
            block_hover_brush_ = new SolidBrush(Color.Silver);
            block_flipped_brush_ = new SolidBrush(Color.White);
        }

        static Image[] status_images_;
        static Image[] number_icons_;
        static Brush block_cover_brush_;
        static Brush block_hover_brush_;
        static Brush block_flipped_brush_;

        public static int OffsetX { get; set; }
        public static int OffsetY { get; set; }
        public static int CellSize { get; set; }
        #endregion

        #region Properties
        public bool IsMine { get; set; }
        public BlockStatus Status { get; set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public uint Number { get; set; }
        public bool IsDisposed { get; private set; }
        public bool Hovered { get; set; }
        public bool ForceShow { get; set; }
        #endregion

        #region Constructors
        public Block(int x, int y, bool is_mine) {
            IsDisposed = false;
            IsMine = is_mine;
            Status = BlockStatus.Normal;
            X = x;
            Y = y;

            block_rectangle_ = new Rectangle(X * (CellSize + 2) + OffsetX + 1, Y * (CellSize + 2) + OffsetY + 1, CellSize, CellSize);

            Number = 0;
        }
        #endregion

        #region Dispose
        public void Dispose() {
            if (IsDisposed)
                return;
            IsDisposed = true;
        }
        #endregion

        #region HitTest
        public bool HitTest(int x, int y) {
            return block_rectangle_.Contains(new Point(x, y));
        }
        #endregion

        #region Render
        public void Render(Graphics g) {
            if (IsDisposed)
                return;
            
            switch (Status) {
                case BlockStatus.Normal :
                    if (Hovered) {
                        g.FillRectangle(block_hover_brush_, block_rectangle_);
                    }
                    else {
                        g.FillRectangle(block_cover_brush_, block_rectangle_);
                    }
                    if (ForceShow) {
                        if (IsMine) {
                            g.DrawImage(status_images_[0], block_rectangle_);
                        }
                        if (Number > 0)
                            g.DrawImage(number_icons_[Number - 1], block_rectangle_);
                    }
                    return;
                case BlockStatus.Flipped :
                    g.FillRectangle(block_flipped_brush_, block_rectangle_);
                    if (Number == 0) {
                        return;
                    }
                    g.DrawImage(number_icons_[Number - 1], block_rectangle_);
                    return;
                case BlockStatus.Exploded :
                    g.FillRectangle(block_flipped_brush_, block_rectangle_);
                    g.DrawImage(status_images_[1], block_rectangle_);
                    return;
                case BlockStatus.Marked :
                    if (Hovered) {
                        g.FillRectangle(block_hover_brush_, block_rectangle_);
                    }
                    else {
                        g.FillRectangle(block_cover_brush_, block_rectangle_);
                    }
                    g.DrawImage(status_images_[2], block_rectangle_);
                    return;
                case BlockStatus.WrongMark :
                    if (Hovered) {
                        g.FillRectangle(block_hover_brush_, block_rectangle_);
                    }
                    else {
                        g.FillRectangle(block_cover_brush_, block_rectangle_);
                    }
                    g.DrawImage(status_images_[3], block_rectangle_);
                    return;
                case BlockStatus.Question :
                    if (Hovered) {
                        g.FillRectangle(block_hover_brush_, block_rectangle_);
                    }
                    else {
                        g.FillRectangle(block_cover_brush_, block_rectangle_);
                    }
                    g.DrawImage(status_images_[4], block_rectangle_);
                    return;
            }
        }
        #endregion

        #region Fields
        Rectangle block_rectangle_;
        #endregion
    }
}
