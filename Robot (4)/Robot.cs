using System;
using RobotContracts;

namespace Robot
{
    public class Robot : IRobot
    {
        public string Name
        {
            get
            {
                return "Dergachev";
            }
        }
        public int Colour
        {
            get
            {
                return 2;
            }
        }

        public class coords
        {
            public int x;
            public int y;
        }
        
        /*поиск минимального пути до цели*/
        public int TakeDistance(int x1, int y1, int x2, int y2)
        {
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            return (int)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
        }

        /*пифагор*/
        int pifagor(int x, int y)
        {
            return (int)Math.Sqrt(x * x + y * y);
        }

        // в какую сторону смещение
        int Sign(int a)
        {
            if (a > 0)
                return 1;
            if (a < 0)
                return -1;
            else
                return 0;
        }

        /*движение к ближайшей точке*/
        public coords MoveTo (RobotState self, RoundConfig config, coords coords)
        {
            /*максимальная дальность перемещения*/
            int maxdistance = 10 * config.max_speed * self.speed / config.max_health * self.energy / config.max_energy;
            
            coords NewMoveToPoint = new coords();
            coords finalcoords = new coords(); // res_cs
            coords testcoords = new coords();
            int dx = coords.x - self.X;
            int dy = coords.y - self.Y;
            bool sw = false;
            coords sign_cd = new coords();
            sign_cd.x = Sign(dx);
            sign_cd.y = Sign(dy);

            finalcoords.x = 0;
            finalcoords.y = 0;
            if (TakeDistance(self.X, self.Y, coords.x, coords.y) < maxdistance)
            {
                finalcoords.x = dx;
                finalcoords.y = dy;
            }
            else
            {
                testcoords.x = 0;
                testcoords.y = 0;

                while (pifagor(testcoords.x, testcoords.y) <= maxdistance)
                {
                    if (sw)
                    {
                        testcoords.x += sign_cd.x;
                        if (pifagor(testcoords.x, testcoords.y) > maxdistance)
                            break;
                        else
                            finalcoords.x += sign_cd.x;
                    }
                    else
                    {
                        testcoords.y += sign_cd.y;
                        if (pifagor(testcoords.x, testcoords.y) > maxdistance)
                            break;
                        else
                            finalcoords.y += sign_cd.y;
                    }
                    sw = !sw;
                }

            }
            return finalcoords;
        }

        public RobotAction Tick(int robotId, RoundConfig config, GameState state)  // ЕБАНЫЙ ТИККК
        {
            int MinDistance = 999999;
            coords PointCoords = new coords();
            RobotState self = state.robots[robotId];
            RobotAction action = new RobotAction();

            // расстояние до енергии
            foreach (Point P in state.points)
            {
                int a = TakeDistance(self.X, self.Y, P.X, P.Y);
                if (P.type == PointType.Energy && (a < MinDistance))
                {
                    MinDistance = a;
                    PointCoords.x = P.X;
                    PointCoords.y = P.Y;
                }
            }
            coords destination = new coords();
            destination = MoveTo(self, config, PointCoords);

            // расстояние до жизней
            foreach (Point P in state.points)
            {
                int a = TakeDistance(self.X, self.Y, P.X, P.Y);
                if (P.type == PointType.Health && (a < MinDistance))
                {
                    MinDistance = a;
                    PointCoords.x = P.X;
                    PointCoords.y = P.Y;
                }
            }
            coords destination2 = new coords();
            destination2 = MoveTo(self, config, PointCoords);

            action.dX = destination.x;
            action.dY = destination.y;

            //защита
            int maxdefdistance = 10 * config.max_radius * self.speed / config.max_health * self.energy / config.max_energy;
            bool attacked = false; //under_attack
            int enemy_id = -1;
            for (int id = 0; id < state.robots.Count; id++)
            {
                RobotState rstates = state.robots[id];
                if (rstates.name != self.name)
                {
                    int enemy_distance_attack = 10 * config.max_radius * rstates.speed / config.max_health * rstates.energy / config.max_energy;
                    int distance = TakeDistance(self.X, self.Y, rstates.X, rstates.Y);
                    if (distance <= enemy_distance_attack && distance <= maxdefdistance)
                    {
                        attacked = true;
                        enemy_id = id;
                        action.targetId = enemy_id;
                    }
                }
            }

            if ((self.attack + self.defence + self.speed) < 0.4 * config.max_health)
            {
                action.dX = destination2.x;
                action.dY = destination2.y;
            } else if (self.energy < 0.4 * config.max_energy)
            {
                action.dX = destination.x;
                action.dY = destination.y;
            }


            return action;
        }
    }
}
