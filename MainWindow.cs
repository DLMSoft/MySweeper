using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;

namespace DLMSoft.MySweeper {
    public class MainWindow : Form {
        #region Constants
        public const int kCellSize = 20;
        #endregion

        #region Constructor
        public MainWindow() {
            TotalMines = 10;
            CellWidth = 9;
            CellHeight = 9;

            Text = "My Sweeper";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ClientSize = ComputeClientSize();
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;

            status_ = GameStatus.Idle;
        }
        #endregion

        #region ComputeClientSize
        Size ComputeClientSize() {
            var width = CellWidth * (kCellSize + 2) + 32;
            var height = CellHeight * (kCellSize + 2) + 32 + 48;
            return new Size(width, height);
        }
        #endregion

        #region Conversion
        public int GetIndexFromPos(int x, int y) {
            if (x < 0 || y < 0 || x >= CellWidth || y >= CellHeight)
                return -1;
            return y * CellWidth + x;
        }

        public Point GetPosFromIndex(int index) {
            if (index < 0 || index >= CellWidth * CellHeight)
                return new Point(-1, -1);

            return new Point(index % CellWidth, index / CellWidth);
        }
        #endregion

        #region Properties
        public int TotalMines { get; set; }
        public int CellWidth { get; set; }
        public int CellHeight { get; set; }
        #endregion

        #region LoadResources
        void LoadResources() {
            var icons_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MySweeper.assets.icons.png");
            var icons = Image.FromStream(icons_stream);
            
            Block.LoadResources(icons);

            block_area_brush_ = new SolidBrush(Color.DimGray);
        }
        #endregion

        #region NewRound
        public void NewRound(int x, int y) {
            status_ = GameStatus.Playing;

            Random random = new Random();

            // Generate mines
            for (var i = 0; i < TotalMines; i++) {
                int mine_index;

                do {
                    mine_index = random.Next(0, blocks_.Length - 1);
                } while((blocks_[mine_index].X == x && blocks_[mine_index].Y == y) || blocks_[mine_index].IsMine);
                // Should not generate mine on first clicked cell.

                blocks_[mine_index].IsMine = true;
            }
            
            // Calculate numbers
            for (var i = 0; i < blocks_.Length; i++) {
                if (blocks_[i].IsMine)
                    continue;
                
                var block = blocks_[i];

                uint count = 0;
                var target_index = 0;
                
                // 4
                target_index = GetIndexFromPos(block.X - 1, block.Y);
                if (target_index != -1 && blocks_[target_index].IsMine) {
                    count++;
                }
                // 7
                target_index = GetIndexFromPos(block.X - 1, block.Y - 1);
                if (target_index != -1 && blocks_[target_index].IsMine) {
                    count++;
                }
                // 8
                target_index = GetIndexFromPos(block.X, block.Y - 1);
                if (target_index != -1 && blocks_[target_index].IsMine) {
                    count++;
                }
                // 9
                target_index = GetIndexFromPos(block.X + 1, block.Y - 1);
                if (target_index != -1 && blocks_[target_index].IsMine) {
                    count++;
                }
                // 6
                target_index = GetIndexFromPos(block.X + 1, block.Y);
                if (target_index != -1 && blocks_[target_index].IsMine) {
                    count++;
                }
                // 3
                target_index = GetIndexFromPos(block.X + 1, block.Y + 1);
                if (target_index != -1 && blocks_[target_index].IsMine) {
                    count++;
                }
                // 2
                target_index = GetIndexFromPos(block.X, block.Y + 1);
                if (target_index != -1 && blocks_[target_index].IsMine) {
                    count++;
                }
                // 1
                target_index = GetIndexFromPos(block.X - 1, block.Y + 1);
                if (target_index != -1 && blocks_[target_index].IsMine) {
                    count++;
                }

                block.Number = count;
            }
        }
        #endregion

        #region Initialize
        public void Initialize() {
            Block.OffsetX = 16;
            Block.OffsetY = 64;
            Block.CellSize = kCellSize;

            status_ = GameStatus.Idle;
            Update();
            Invalidate();

            marked_blocks_ = 0;
            remean_blocks_ = CellWidth * CellHeight;

            LoadResources();
            blocks_ = new Block[CellWidth * CellHeight];
            
            for (var i = 0; i < blocks_.Length; i++) {
                var block = new Block(i % CellWidth, i / CellWidth, false);
                //block.ForceShow = true;
                blocks_[i] = block;
            }
        }
        #endregion

        #region Render
        void Render(Graphics g) {
            g.Clear(Color.Silver);

            // draw blocks
            g.FillRectangle(block_area_brush_, 16, 64, CellWidth * (kCellSize + 2), CellHeight * (kCellSize + 2));

            if (blocks_ != null) {
                foreach(var block in blocks_) {
                    if (block == null)
                        continue;
                    
                    block.Render(g);
                }
            }
        }
        #endregion

        #region Cleanup
        void Cleanup() {
            foreach (var b in blocks_) {
                b.Dispose();
            }
        }
        #endregion

        #region DoWin
        void DoWin() {
            var mines = from b in blocks_ where b.IsMine && b.Status != BlockStatus.Marked select b;
            foreach (var m in mines) {
                m.Status = BlockStatus.Marked;
            }
            status_ = GameStatus.Win;

            MessageBox.Show("您获胜了！", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region DoLose
        void DoLose() {
            var mines = from b in blocks_ where b.IsMine && b.Status != BlockStatus.Marked select b;
            foreach (var m in mines) {
                m.ForceShow = true;
            }
            var wrongs = from b in blocks_ where !b.IsMine && b.Status == BlockStatus.Marked select b;
            foreach (var b in wrongs) {
                b.Status = BlockStatus.WrongMark;
            }
            status_ = GameStatus.Lose;

            MessageBox.Show("您失败了！", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region CheckForWin
        void CheckForWin() {
            if (status_ != GameStatus.Playing)
                return;

            if (remean_blocks_ == TotalMines) { // Win !
                DoWin();
            }
        }
        #endregion

        #region FlipBlock
        void FlipBlock(Block block) {
            if (block.Status != BlockStatus.Normal && block.Status != BlockStatus.Question)
                return;

            if (block.IsMine) {
                // KABOOM !
                block.Status = BlockStatus.Exploded;
                DoLose();
                return;
            }
            // Flip clicked block
            block.Status = BlockStatus.Flipped;
            remean_blocks_ --;
            CheckForWin();

            // Flip around if is 0
            if (block.Number == 0)
                FlipAround(block);
        }
        #endregion

        #region FlipAround
        void FlipAround(Block block) {
            int target_index;
            
            // 4
            target_index = GetIndexFromPos(block.X - 1, block.Y);
            if (target_index != -1) {
                var target_block = blocks_[target_index];
                FlipBlock(target_block);
            }
            // 7
            target_index = GetIndexFromPos(block.X - 1, block.Y - 1);
            if (target_index != -1) {
                var target_block = blocks_[target_index];
                FlipBlock(target_block);
            }
            // 8
            target_index = GetIndexFromPos(block.X, block.Y - 1);
            if (target_index != -1) {
                var target_block = blocks_[target_index];
                FlipBlock(target_block);
            }
            // 9
            target_index = GetIndexFromPos(block.X + 1, block.Y - 1);
            if (target_index != -1) {
                var target_block = blocks_[target_index];
                FlipBlock(target_block);
            }
            // 6
            target_index = GetIndexFromPos(block.X + 1, block.Y);
            if (target_index != -1) {
                var target_block = blocks_[target_index];
                FlipBlock(target_block);
            }
            // 3
            target_index = GetIndexFromPos(block.X + 1, block.Y + 1);
            if (target_index != -1) {
                var target_block = blocks_[target_index];
                FlipBlock(target_block);
            }
            // 2
            target_index = GetIndexFromPos(block.X, block.Y + 1);
            if (target_index != -1) {
                var target_block = blocks_[target_index];
                FlipBlock(target_block);
            }
            // 1
            target_index = GetIndexFromPos(block.X - 1, block.Y + 1);
            if (target_index != -1) {
                var target_block = blocks_[target_index];
                FlipBlock(target_block);
            }
        }
        #endregion

        #region CountAroundMarked
        int CountAroundMarked(Block block) {
            int count = 0;
            var target_index = -1;
            
            // 4
            target_index = GetIndexFromPos(block.X - 1, block.Y);
            if (target_index != -1 && blocks_[target_index].Status == BlockStatus.Marked) {
                count++;
            }
            // 7
            target_index = GetIndexFromPos(block.X - 1, block.Y - 1);
            if (target_index != -1 && blocks_[target_index].Status == BlockStatus.Marked) {
                count++;
            }
            // 8
            target_index = GetIndexFromPos(block.X, block.Y - 1);
            if (target_index != -1 && blocks_[target_index].Status == BlockStatus.Marked) {
                count++;
            }
            // 9
            target_index = GetIndexFromPos(block.X + 1, block.Y - 1);
            if (target_index != -1 && blocks_[target_index].Status == BlockStatus.Marked) {
                count++;
            }
            // 6
            target_index = GetIndexFromPos(block.X + 1, block.Y);
            if (target_index != -1 && blocks_[target_index].Status == BlockStatus.Marked) {
                count++;
            }
            // 3
            target_index = GetIndexFromPos(block.X + 1, block.Y + 1);
            if (target_index != -1 && blocks_[target_index].Status == BlockStatus.Marked) {
                count++;
            }
            // 2
            target_index = GetIndexFromPos(block.X, block.Y + 1);
            if (target_index != -1 && blocks_[target_index].Status == BlockStatus.Marked) {
                count++;
            }
            // 1
            target_index = GetIndexFromPos(block.X - 1, block.Y + 1);
            if (target_index != -1 && blocks_[target_index].Status == BlockStatus.Marked) {
                count++;
            }

            return count;
        }
        #endregion

        #region Events handling
        #region OnLoad
        protected override void OnLoad(EventArgs e) {
            Initialize();
        }
        #endregion

        #region OnPaint
        protected override void OnPaint(PaintEventArgs e) {
            Render(e.Graphics);

            if (status_ != GameStatus.Idle && status_ != GameStatus.Playing) {
                this.Update();
                return;
            }

            this.Update();
            this.Invalidate();
        }
        #endregion
        
        #region OnMouseMove
        protected override void OnMouseMove(MouseEventArgs e) {
            foreach (var block in blocks_) {
                block.Hovered = block.HitTest(e.X, e.Y);
            }
        }
        #endregion
        
        #region OnMouseClick
        protected override void OnMouseClick(MouseEventArgs e) {
            if (status_ != GameStatus.Playing && status_ != GameStatus.Idle) {
                Cleanup();
                Initialize();
                return;
            }

            foreach (var block in blocks_) {
                if (block.HitTest(e.X, e.Y)) {
                    if (e.Button == MouseButtons.Left) {
                        if (status_ != GameStatus.Playing) {
                            NewRound(block.X, block.Y);
                        }
                        
                        if (block.Status == BlockStatus.Flipped && block.Number != 0) {
                            if (CountAroundMarked(block) != block.Number)
                                return;
                            FlipAround(block);
                            return;
                        }

                        FlipBlock(block);
                        return;
                    }
                    if (e.Button == MouseButtons.Right) {
                        if (block.Status == BlockStatus.Normal) {
                            block.Status = BlockStatus.Marked;
                            return;
                        }
                        if (block.Status == BlockStatus.Marked) {
                            block.Status = BlockStatus.Question;
                            return;
                        }
                        if (block.Status == BlockStatus.Question) {
                            block.Status = BlockStatus.Normal;
                            return;
                        }
                        return;
                    }
                    return;
                }
            }
        }
        #endregion
        #endregion

        #region Fields
        Brush block_area_brush_;
        Block[] blocks_;
        GameStatus status_;
        int marked_blocks_;
        int remean_blocks_;
        #endregion
    }
}
