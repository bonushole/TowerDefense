using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TowerDefense
{
    public class TowerDefense : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch batch;
        SpriteFont text;
        MouseHandler mouse;
        Texture2D defaultMouse;
        Texture2D enemy;
        Texture2D banner;
        Texture2D banner2;
        Texture2D[] proj;
        SoundEffect[] sound;
        Viewport viewport;
        Button startButton;
        Button upgradeButton;
        List < Button >buttonlist;
        List< Tower > towerlist;
        List< Enemy > enemylist;
        List < Projectile > projectilelist;
        MessageLog MessageLog = new MessageLog();
        Node[,] nodes = new Node[Constants.X + 1, Constants.Y + 1];
        bool attackPhase = false;
        bool playerLoses = false;
        int level = 0;
        int enemyID = 0;
        int gold;
        double lastSpawnedTime = 0;

        public TowerDefense()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 950;
            graphics.PreferredBackBufferHeight = 800;
        }


        protected override void Initialize()
        {
            viewport = graphics.GraphicsDevice.Viewport;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            batch = new SpriteBatch(GraphicsDevice);
            text = Content.Load<SpriteFont>("text");
            startButton = new Button(new Vector2(10 + 32, viewport.Height * .2f - 74), Content.Load<Texture2D>(@"start"), false, 0);
            upgradeButton = new Button(new Vector2(viewport.Width - 160, viewport.Height * .55f), Content.Load<Texture2D>(@"upgrade"), false, 0);
            enemy = Content.Load<Texture2D>(@"enemy");
            banner = Content.Load<Texture2D>(@"banner");
            banner2 = Content.Load<Texture2D>(@"banner2");
            this.buttonlist = new List<Button>();
            this.buttonlist.Add(new Button(new Vector2(10, viewport.Height * .2f), Content.Load<Texture2D>(@"generic tower"), false, 1));
            this.buttonlist.Add(new Button(new Vector2(10 + 64, viewport.Height * .2f), Content.Load<Texture2D>(@"cannon tower"), false, 2));
            this.buttonlist.Add(new Button(new Vector2(10, (viewport.Height * .2f) + 64), Content.Load<Texture2D>(@"battery tower"), false, 3));
            this.buttonlist.Add(new Button(new Vector2(10 + 64, (viewport.Height * .2f) + 64), Content.Load<Texture2D>(@"blast tower"), false, 4));
            this.buttonlist.Add(new Button(new Vector2(10 + 16, (viewport.Height * .5f)), Content.Load<Texture2D>(@"wall"), true, 5));
            this.buttonlist.Add(new Button(new Vector2(10 + 64, (viewport.Height * .5f)), Content.Load<Texture2D>(@"portal"), true, 6));

            this.towerlist = new List<Tower>();
            this.projectilelist = new List<Projectile>();
            this.enemylist = new List<Enemy>();

            this.proj = new Texture2D[4];
            proj[0] = Content.Load<Texture2D>(@"bullet");
            proj[1] = Content.Load<Texture2D>(@"cannon ball");
            proj[2] = Content.Load<Texture2D>(@"lightning bolt");
            proj[3] = Content.Load<Texture2D>(@"blast");

            this.sound = new SoundEffect[20];
            sound[0] = Content.Load<SoundEffect>(@"generic attack");
            sound[1] = Content.Load<SoundEffect>(@"cannon attack");
            sound[2] = Content.Load<SoundEffect>(@"battery attack");
            sound[3] = Content.Load<SoundEffect>(@"blast attack");
            sound[4] = Content.Load<SoundEffect>(@"damaged");
            sound[5] = Content.Load<SoundEffect>(@"sell");
            sound[6] = Content.Load<SoundEffect>(@"wallsound");
            sound[7] = Content.Load<SoundEffect>(@"portalsound");

            gold = Constants.STARTINGGOLD;

            int actualY = 64;
            int actualX = 148;
            for (int i = 0; i <= Constants.X; i++)
            {
                for (int j = 0; j <= Constants.Y; j++)
                {
                    nodes[i, j] = new Node(new Vector2(actualX, actualY), new Vector2(i, j), Content.Load<Texture2D>(@"grass"));

                    actualY = actualY + 32;
                    if (actualY > 704)
                    {
                        actualY = 64;
                        actualX = actualX + 32;
                    }
                }
            }

            defaultMouse = Content.Load<Texture2D>(@"cursor");
            mouse = new MouseHandler(new Vector2(0, 0), defaultMouse);
        }

        protected override void Update(GameTime gameTime)
        {
            HandleMouse();
            mouse.Update();
            if (attackPhase)
            {
                foreach (Enemy e in enemylist)
                {
                    if (!e.spawned && (gameTime.TotalGameTime.TotalSeconds - lastSpawnedTime) > e.spawnRate)
                    {
                        e.spawn();
                        lastSpawnedTime = gameTime.TotalGameTime.TotalSeconds;
                    }
                }
                List<Enemy> temp = new List<Enemy>();
                foreach (Enemy e in enemylist)
                {
                    if (e.dead)
                    {
                        temp.Add(e);
                    }
                    e.move();
                    if (e.lose)
                    {
                        MessageLog.GameOver();
                        playerLoses = true;
                    }
                }
                foreach (Enemy e in temp)
                {
                    enemylist.Remove(e);
                }
                foreach (Tower t in towerlist)
                {
                    projectilelist = t.Attack(enemylist, projectilelist, gameTime.TotalGameTime.TotalSeconds);
                }
                List<Projectile> temp2 = new List<Projectile>();
                foreach (Projectile p in projectilelist)
                {
                    if (p.Move())
                    {
                        temp2.Add(p);
                    }
                    
                }
                foreach (Projectile p in temp2)
                {
                    projectilelist.Remove(p);
                }
            }
            if (enemylist.Count == 0 && attackPhase)
            {
                attackPhase = false;
                projectilelist.Clear();
                MessageLog.LevelComplete(level * 2 + (int)(gold * .05f), level);
                gold = gold + (level * 2 + (int)(gold * .05f));
            }
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.WhiteSmoke);

            batch.Begin();
            for (int i = 0; i <= Constants.X; i++)
                for( int j = 0; j <= Constants.Y; j++ )
                    nodes[i,j].Draw(batch);

            startButton.Draw(batch);
            enemylist.ForEach(e => e.Draw(batch));
            buttonlist.ForEach(b => b.Draw(batch));
            towerlist.ForEach(t => t.Draw(batch));
            projectilelist.ForEach(p => p.Draw(batch));

            if (mouse.towerSelected != null)
                mouse.towerSelected.ShowStats(batch, text, viewport);
            else if (mouse.towerClicked != null)
            {
                upgradeButton.Draw(batch);
                mouse.towerClicked.ShowStats(batch, text, viewport);
            }

            if (mouse.enemyHovered != null)
                mouse.enemyHovered.ShowStats(batch, text, viewport);

            batch.DrawString(text, "GOLD - " + gold + " $", new Vector2(viewport.Width *.8f, viewport.Height *.1f), Color.Black,
                    0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);

            MessageLog.Draw(batch, text, viewport);

            batch.Draw(banner, new Vector2(viewport.Width / 2 - banner.Width / 2, 0), null, Color.White);
            batch.Draw(banner2, new Vector2(viewport.Width / 2 - banner.Width / 4, viewport.Height - banner2.Height), null, Color.White);
            mouse.Draw(batch);

            batch.End();

            base.Draw(gameTime);
        }

        private void HandleMouse()
        {
            if (playerLoses)
            {
                if (mouse.mouseState.LeftButton == ButtonState.Pressed)
                {
                    Exit();
                }
                return;
            }

            if (mouse.mouseState.LeftButton == ButtonState.Pressed && startButton.ContainsPoint(mouse.pos) && !attackPhase && !mouse.portalClicked)
            {
                StartLevel();
            }

            if (attackPhase)
            {
                HandleAttackPhase();
            }
            else
            { 
                HandleMouseHover();
                if (mouse.mouseState.LeftButton == ButtonState.Pressed)
                {
                    HandleLeftClick();
                }
                if (mouse.mouseState.RightButton == ButtonState.Pressed && !mouse.rClicking && mouse.nodeHovered != null)
                {
                    HandleRightClick();
                }
                if (mouse.mouseState.LeftButton == ButtonState.Released)
                {
                    mouse.clicking = false;
                }
                if (mouse.mouseState.RightButton == ButtonState.Released)
                {
                    mouse.rClicking = false;
                }
            }
        }

        private void HandleAttackPhase()
        {
            mouse.hovering = false;
            mouse.enemyHovered = null;
            foreach (Enemy e in enemylist)
            {
                if (e.ContainsPoint(mouse.pos))
                {
                    e.hovering = true;
                    mouse.hovering = true;
                    mouse.enemyHovered = e;
                }
                else
                {
                    e.hovering = false;
                    e.color = Color.White;
                }
            }
            if (mouse.enemyHovered != null)
            {
                mouse.enemyHovered.color = Color.Yellow;
            }
        }

        private void HandleMouseHover()
        {
            mouse.hovering = false;
            mouse.buttonHovered = null;
            mouse.towerHovered = null;
            mouse.nodeHovered = null;

            foreach (Button b in buttonlist)
            {
                if (b.ContainsPoint(mouse.pos))
                {
                    b.hovering = true;
                    mouse.hovering = true;
                    mouse.buttonHovered = b;
                }
                else
                {
                    b.hovering = false;
                    b.color = Color.White;
                }

            }
            foreach (Tower t in towerlist)
            {
                if (t.ContainsPoint(mouse.pos) && !mouse.hovering)
                {
                    t.hovering = true;
                    mouse.hovering = true;
                    mouse.towerHovered = t;
                }
                else
                {
                    t.hovering = false;
                    t.color = Color.White;
                }
            }

            for (int i = 0; i <= Constants.X; i++)
            {
                for (int j = 0; j <= Constants.Y; j++)
                {
                    if (nodes[i, j].ContainsPoint(mouse.pos))
                    {
                        nodes[i, j].hovering = true;
                        mouse.hovering = true;
                        mouse.nodeHovered = nodes[i, j];
                    }
                    else
                    {
                        nodes[i, j].hovering = false;
                        nodes[i, j].color = Color.White;
                    }
                }
                if (mouse.enemyHovered != null)
                {
                    mouse.enemyHovered.color = Color.Green;
                }
            }
            if (mouse.buttonHovered != null)
            {
                mouse.buttonHovered.color = Color.Green;
            }
            if (mouse.towerClicked != null)
            {
                mouse.towerClicked.color = Color.Green;
            }
            else if (mouse.highlight && mouse.towerSelected == null && mouse.towerHovered != null)
            {
                mouse.towerHovered.color = Color.Green;
            }
            else if (mouse.highlight && mouse.wallClicked && mouse.nodeHovered != null && !mouse.nodeHovered.portal && !mouse.nodeHovered.wall && CheckForPath((int)mouse.nodeHovered.simplePos.X, (int)mouse.nodeHovered.simplePos.Y, false, false))
            {
                mouse.nodeHovered.color = Color.Green;
            }
            else if (mouse.highlight && !mouse.wallClicked && mouse.nodeHovered != null && !mouse.nodeHovered.portal && !mouse.nodeHovered.wall)
            {
                mouse.nodeHovered.color = Color.Green;
            }
            else if (mouse.highlight && mouse.nodeHovered != null && mouse.nodeHovered.portal && !mouse.nodeHovered.wall)
            {
                mouse.nodeHovered.color = Color.Green;
                if (mouse.nodeHovered.portalsTo != null)
                    mouse.nodeHovered.portalsTo.color = Color.Green;
            }
            else if (mouse.highlight && mouse.nodeHovered != null && !mouse.nodeHovered.portal && mouse.nodeHovered.wall)
            {
                mouse.nodeHovered.color = Color.Red;
            }
            else if (mouse.highlight && mouse.nodeHovered != null && !CheckForPath((int)mouse.nodeHovered.simplePos.X, (int)mouse.nodeHovered.simplePos.Y, false, false))
            {
                mouse.nodeHovered.color = Color.Red;
            }
        }

        private void StartLevel()
        {
            mouse.UpdateTex(defaultMouse);
            mouse.towerClicked = null;
            mouse.towerSelected = null;
            attackPhase = true;
            level++;
            MessageLog.Level(level);
            Random rand = new Random();
            double num = rand.NextDouble();
            if (num < .3)
            {
                for (int i = 0; i < (15 + level); i++)
                {
                    enemylist.Add(new Enemy(level * 5, 1, enemy, nodes, "Malaria", 1.0f, .25, sound[4], sound[7], enemyID));
                    enemyID++;
                }
            }
            else if (num < .6)
            {
                for (int i = 0; i < (30 + 2 * level); i++)
                {
                    enemylist.Add(new Enemy(level * 3, 2, enemy, nodes, "Tuberculosis", .75f, .10, sound[4], sound[7], enemyID));
                    enemyID++;
                }
            }
            else
            {
                for (int i = 0; i < (5 + level / 2); i++)
                {
                    enemylist.Add(new Enemy(level * 20, 1, enemy, nodes, "AIDS", 1.25f, .50, sound[4], sound[7], enemyID));
                    enemyID++;
                }
            }
        }

        private void HandleLeftClick()
        {
            if (upgradeButton.ContainsPoint(mouse.pos) && mouse.towerClicked != null && gold >= mouse.towerClicked.cost && !mouse.clicking)
            {
                gold = gold - mouse.towerClicked.cost;
                mouse.towerClicked.upgrade();
            }
            if (mouse.buttonHovered != null && !mouse.clicking)
            {
                mouse.highlight = mouse.buttonHovered.highlight;
                mouse.towerID = mouse.buttonHovered.ID;
                mouse.UpdateTex(mouse.buttonHovered.tex);
                switch (mouse.towerID)
                {
                    case 1:
                        mouse.towerSelected = new GenericTower(mouse.pos, mouse.tex, proj[0], sound[0]);
                        break;
                    case 2:
                        mouse.towerSelected = new CannonTower(mouse.pos, mouse.tex, proj[1], sound[1]);
                        break;
                    case 3:
                        mouse.towerSelected = new BatteryTower(mouse.pos, mouse.tex, proj[2], sound[2]);
                        break;
                    case 4:
                        mouse.towerSelected = new BlastTower(mouse.pos, mouse.tex, proj[3], sound[3]);
                        break;
                    case 5:
                        mouse.wallClicked = true;
                        break;
                    case 6:
                        mouse.portalClicked = true;
                        break;
                    default:
                        break;
                }
                mouse.towerClicked = null;
            }
            else if (mouse.nodeHovered != null && !mouse.highlight && mouse.towerSelected != null && !mouse.clicking && mouse.pos.X <= 641 && mouse.pos.Y <= 679)
            {
                if (gold >= mouse.towerSelected.cost)
                {
                    gold = gold - mouse.towerSelected.cost;
                    towerlist.Add(mouse.towerSelected);
                    mouse.towerSelected.position = mouse.pos;
                    mouse.towerSelected = null;
                    mouse.UpdateTex(defaultMouse);
                    mouse.highlight = true;
                    mouse.towerClicked = null;
                    sound[6].Play();
                }
                else
                {
                    MessageLog.NotEnoughGold();
                }
            }
            else if (mouse.nodeHovered != null && !mouse.nodeHovered.wall && !mouse.nodeHovered.portal && mouse.highlight && mouse.wallClicked && CheckForPath((int)mouse.nodeHovered.simplePos.X, (int)mouse.nodeHovered.simplePos.Y, false, false))
            {
                if (gold >= 1)
                {
                    mouse.nodeHovered.wall = true;
                    mouse.nodeHovered.UpdateTex(mouse.tex);
                    gold = gold - 1;
                    sound[6].Play();
                }
                else
                {
                    MessageLog.NotEnoughGold();
                }

            }
            else if (mouse.nodeHovered != null && mouse.portalComplete && !mouse.nodeHovered.wall && !mouse.nodeHovered.portal && mouse.highlight && mouse.portalClicked)
            {
                mouse.nodeHovered.portal = true;
                mouse.nodeHovered.UpdateTex(mouse.tex);
                mouse.portalLocation = mouse.nodeHovered;
                mouse.portalComplete = false;
            }
            else if (mouse.nodeHovered != null && !mouse.portalComplete && !mouse.nodeHovered.wall && !mouse.nodeHovered.portal && mouse.highlight && mouse.portalClicked && CheckForPath((int)mouse.nodeHovered.simplePos.X, (int)mouse.nodeHovered.simplePos.Y, true, false))
            {
                if (gold >= 20)
                {
                    mouse.nodeHovered.portal = true;
                    mouse.nodeHovered.UpdateTex(mouse.tex);
                    mouse.nodeHovered.portalsTo = mouse.portalLocation;
                    mouse.portalLocation.portalsTo = mouse.nodeHovered;
                    mouse.portalComplete = true;
                    gold = gold - 20;
                }
                else
                {
                    MessageLog.NotEnoughGold();
                }
            }
            else if (mouse.towerHovered != null && mouse.highlight && mouse.towerSelected == null && !mouse.clicking)
            {
                mouse.towerClicked = mouse.towerHovered;
            }
            mouse.clicking = true;
        }

        private void HandleRightClick()
        {
            if (mouse.towerHovered != null)
            {
                gold = gold + mouse.towerHovered.cost;
                towerlist.Remove(mouse.towerHovered);
                sound[5].Play();
            }
            else if (mouse.nodeHovered.wall && CheckForPath((int)mouse.nodeHovered.simplePos.X, (int)mouse.nodeHovered.simplePos.Y, false, true))
            {
                gold = gold + 1;
                mouse.nodeHovered.wall = false;
                mouse.nodeHovered.defaultSet();
                sound[6].Play();
            }
            else if (mouse.nodeHovered.portal && CheckForPath((int)mouse.nodeHovered.simplePos.X, (int)mouse.nodeHovered.simplePos.Y, true, true))
            {
                mouse.nodeHovered.portal = false;
                mouse.nodeHovered.defaultSet();
                if (mouse.nodeHovered.portalsTo != null)
                {
                    mouse.nodeHovered.portalsTo.portal = false;
                    mouse.nodeHovered.portalsTo.portalsTo = null;
                    mouse.nodeHovered.portalsTo.defaultSet();
                    mouse.nodeHovered.portalsTo = null;
                    gold = gold + 20;
                }
                else
                {
                    mouse.portalComplete = true;
                }
            }
            if (!mouse.portalComplete)
            {
                mouse.portalLocation.portal = false;
                mouse.portalLocation.defaultSet();
                mouse.portalLocation = null;
                mouse.portalComplete = true;
            }
            mouse.UpdateTex(defaultMouse);
            mouse.wallClicked = false;
            mouse.portalClicked = false;
            mouse.towerClicked = null;
            mouse.towerSelected = null;
            mouse.highlight = true;
            mouse.rClicking = true;
        }

        private static int heuristic(Node current)
        {
            return Constants.Y - (int)current.simplePos.Y;
        }

        internal static List<Node> findBestPath(Node[,] nodes)
        {
            List<Node> available = new List<Node>();
            HashSet<Node> visited = new HashSet<Node>();

            for (int i = 0; i <= Constants.X; i++)
                for (int j = 0; j <= Constants.Y; j++)
                {
                    nodes[i, j].parent = null;
                    nodes[i, j].fScore = int.MaxValue;
                }
            for (int i = 0; i <= Constants.X; i++)
            {
                if (!nodes[i, 0].wall)
                {
                    available.Add(nodes[i, 0]);
                    nodes[i, 0].fScore = 0;
                }
            }
            while (available.Count != 0)
            {
                Node current = available.OrderBy(n => n.fScore).First();
                if (current.simplePos.Y == Constants.Y)
                {
                    List<Node> bestPath = new List<Node>();
                    while (current.parent != null)
                    {
                        bestPath.Add(current.parent);
                        current = current.parent;
                    }
                    return bestPath;
                }
                available.Remove(current);
                visited.Add(current);
                foreach (Node n in current.getNeighbors(nodes))
                {
                    if (visited.Contains(n))
                    {
                        continue;
                    }
                    int possibleScore = current.gScore + 1;
                    if (!available.Contains(n))
                    {
                        available.Add(n);
                    }
                    else if (possibleScore >= n.gScore)
                    {
                        continue;
                    }
                    n.parent = current;
                    n.gScore = possibleScore;
                    n.fScore = possibleScore + heuristic(n);
                }
            }
            return null;
        }

        public bool CheckForPath( int x, int y, bool portal, bool remove)
        {
            Node portaledTo = nodes[x, y].portalsTo;
            if (portal)
            {
                nodes[x, y].portal = !remove;
                nodes[x, y].portalsTo = remove ? null : mouse.portalLocation;
                if (remove)
                {
                    portaledTo.portalsTo = null;
                    portaledTo.portal = false;
                }
                else
                {
                    mouse.portalLocation.portalsTo = nodes[x, y];
                }

            }
            else
            {
                nodes[x, y].wall = !remove;
            }
            List<Node> bestPath = findBestPath(nodes);
            if (portal)
            {
                nodes[x, y].portal = remove;
                nodes[x, y].portalsTo = remove ? portaledTo : null;
                if (remove)
                {
                    portaledTo.portalsTo = nodes[x, y];
                    portaledTo.portal = true;
                }
                else
                {
                    mouse.portalLocation.portalsTo = null;
                }
            }
            else
            {
                nodes[x, y].wall = remove;
            }
            return bestPath != null;
        }
    }
}
