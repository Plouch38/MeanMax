using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Utility{
    public static float map(float value, float istart, float istop, float ostart, float ostop) {
    return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
    }
}

public class Coordinate{
    public int X;
    public int Y;

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return "X : " + X + " | Y : " + Y;
    }
}

public abstract class Looter{

    public int unitId;
    public int unitType;
    public int playerId;
    public int throttle;
    public float mass;
    public int radius;
    public float friction;
    public Coordinate position;
    public Coordinate speed;
    public Coordinate acceleration;
    public int waterAvailable;
    public int waterMax;
    public Weapon skill;

    public Looter(int _unitId, int _unitType, int _playerId, float _mass, int _radius, int _x, int _y, int _vx, int _vy, int _extra, int _extra2)
    {
        unitId = _unitId;
        unitType = _unitType;
        playerId = _playerId;
        mass = _mass;
        radius = _radius;
        position = new Coordinate(_x, _y);
        speed = new Coordinate(_vx, _vy);
        waterAvailable = _extra;
        waterMax = _extra2;
    }

    public int DistanceFrom(Coordinate c)
    {
        //Console.Error.WriteLine("Destination : " + c.ToString() + "Position actuelle : " + position.ToString());
        int distance = (int)Math.Sqrt(Math.Pow(c.X - position.X,2) + Math.Pow(c.Y - position.Y,2));
        //Console.Error.WriteLine("Distance : " + distance );
        
        return distance;
    }

    public Coordinate Next()
    {
        int newX = (int)(position.X + (speed.X * (1 - friction)));
        int newY = (int)(position.Y + (speed.Y * (1 - friction)));

        return new Coordinate(newX, newY);        
    }

    public Looter ClosestUnit(List<Looter> unitList, int _unitType)
    {
        int minDistance = 10000;
        Looter target = null;
        List<Looter> list;

        if (_unitType == -1)
        {
            list = unitList;
        }
        else
        {
            list = unitList.Where(x => x.unitType == _unitType).ToList();
        }

        foreach (Looter l in list)
        {
            //Console.Error.WriteLine("Looter in list : " + l.unitId + " Type : " + l.unitType);
            if (DistanceFrom(l.position) < minDistance)
            {
                minDistance = DistanceFrom(l.position);
                target = l;
            }
        }
        //Console.Error.WriteLine("Closest Unit in (" + string.Join(", ", list) + ") : " + target.ToString());

        return target;
    }

    public virtual void Decide()
    {
        Wait();
        Console.Error.WriteLine("Vehicle stalled");
    }

    public static void Go(Coordinate coor, int acc)
    {
        Console.WriteLine(coor.X + " " + coor.Y + " " + acc);
    }

    public static void Wait()
    {
        Console.WriteLine("WAIT");
    }

    public static void Skill(Coordinate coor)
    {
        Console.WriteLine("SKILL " + coor.X + " " + coor.Y);
    }

}

public class Reaper : Looter{
    public Reaper(int _unitId, int _unitType, int _playerId, float _mass, int _radius, int _x, int _y, int _vx, int _vy, int _extra, int _extra2) : base(_unitId, _unitType, _playerId, _mass, _radius, _x, _y, _vx, _vy, _extra, _extra2){
        throttle = 300;
        friction = 0.2f;
    }

    public override void Decide()
    {

        List<Looter> list = Player.units.ToList();

        if(Player.units.Where(x => x.unitType == 4).ToList().Count == 0)
        {
            Go(new Coordinate(0,0), throttle);
            return;
        }
        else
        {
            
            Looter ClosestWreck = null;
    
            while (ClosestWreck == null)
            {
                ClosestWreck = ClosestUnit(list, 4);
                
                foreach(Looter l in Player.units.Where(x => x.playerId != -1).Where(x => x.unitType == 0))
                {
                    if (ClosestWreck == null || list.Count == 0)
                    {
                        ClosestWreck = Player.joueurs[0].units[1];
                        break;
                    }
            
                    if (ClosestWreck.DistanceFrom(l.position) < 200)
                    {
                        list.Remove(ClosestWreck);
                        ClosestWreck = null;
                    }
                }
                
                Console.Error.WriteLine(ClosestWreck);
            }
    
            if (ClosestWreck == null)
            {
                ClosestWreck = (Wreck)ClosestUnit(Player.units, 4);
            }
    
            int acc = (int)Utility.map(DistanceFrom(ClosestWreck.position), 500/mass, 1000/mass, 0, throttle);
            Go(ClosestWreck.position, Math.Max(0, acc));
        }
    }
}

public class Destroyer : Looter{
    public Destroyer(int _unitId, int _unitType, int _playerId, float _mass, int _radius, int _x, int _y, int _vx, int _vy, int _extra, int _extra2) : base(_unitId, _unitType, _playerId, _mass, _radius, _x, _y, _vx, _vy, _extra, _extra2){
        throttle = 300;
        friction = 0.3f;
    }

    public override void Decide()
    {
        Tanker ClosestTanker = (Tanker)ClosestUnit(Player.units, 3);
        int acc = throttle;
        if (ClosestTanker == null)
        {
            Go(new Coordinate(0,0), acc);
        }
        else
        {
        Go(ClosestTanker.position, acc);
        }
    }
}

public class Doof : Looter{
    public Doof(int _unitId, int _unitType, int _playerId, float _mass, int _radius, int _x, int _y, int _vx, int _vy, int _extra, int _extra2) : base(_unitId, _unitType, _playerId, _mass, _radius, _x, _y, _vx, _vy, _extra, _extra2){
        throttle = 300;
        friction = 0.25f;
    }

    public override void Decide()
    {
        Coordinate target;
        target = new Coordinate((int)(Player.joueurs[1].units[0].position.X + Player.joueurs[2].units[0].position.X)/2, (int)(Player.joueurs[1].units[0].position.Y + Player.joueurs[2].units[0].position.Y)/2);
        int distance = DistanceFrom(target);
        int acc = (int)Utility.map(distance, 200/mass, 3000/mass, 0, throttle);
        if (DistanceFrom(target) > 400)
        {
            Go(target, acc);
        }
        else
        {
            Skill(target);
        }
    }
}

public class Tanker : Looter{

    public Tanker(int _unitId, int _unitType, int _playerId, float _mass, int _radius, int _x, int _y, int _vx, int _vy, int _extra, int _extra2) : base(_unitId, _unitType, _playerId, _mass, _radius, _x, _y, _vx, _vy, _extra, _extra2){
        throttle = 500;
        friction = 0.4f;
    }
}

public class Wreck : Looter{
    public Wreck(int _unitId, int _unitType, int _playerId, float _mass, int _radius, int _x, int _y, int _vx, int _vy, int _extra, int _extra2) : base(_unitId, _unitType, _playerId, _mass, _radius, _x, _y, _vx, _vy, _extra, _extra2){
        throttle = 0;
        friction = 0.0f;
    }
}

public abstract class Weapon{
    public string action;
    public int range;
    public int radius;
    public int duration;
}

public class Tar : Weapon{
    public Tar(){
        action = "Tar";
        range = 2000;
        radius = 1000;
        duration = 3;
    }
}

public class Grenades : Weapon{
    public Grenades(){
        action = "Grenades";
        range = 2000;
        radius = 1000;
        duration = 0;
    }
}

public class Oil : Weapon{
    public Oil(){
        action = "Oil";
        range = 2000;
        radius = 1000;
        duration = 3;
    }
}

public class Joueur{
    public int score;
    public int rage;
    public List<Looter> units = new List<Looter>();
    public Joueur(int _score = 0, int _rage = 0)
    {
        score = _score;
        rage = _rage;
    }
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    public static List<Looter> units = new List<Looter>();
    public static List<Joueur> joueurs = new List<Joueur>();


    static void Main(string[] args)
    {

        // game loop
        while (true)
        {
            units.Clear();
            joueurs.Clear();
            joueurs.Add(new Joueur());
            joueurs.Add(new Joueur());
            joueurs.Add(new Joueur());
            
            joueurs[0].score = int.Parse(Console.ReadLine());
            joueurs[1].score = int.Parse(Console.ReadLine());
            joueurs[2].score = int.Parse(Console.ReadLine());
            joueurs[0].rage = int.Parse(Console.ReadLine());
            joueurs[1].rage = int.Parse(Console.ReadLine());
            joueurs[2].rage = int.Parse(Console.ReadLine());

            int unitCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < unitCount; i++)
            {
                
                string[] inputs = Console.ReadLine().Split(' ');
                int unitId = int.Parse(inputs[0]);
                int unitType = int.Parse(inputs[1]);
                int player = int.Parse(inputs[2]);
                float mass = float.Parse(inputs[3]);
                int radius = int.Parse(inputs[4]);
                int x = int.Parse(inputs[5]);
                int y = int.Parse(inputs[6]);
                int vx = int.Parse(inputs[7]);
                int vy = int.Parse(inputs[8]);
                int extra = int.Parse(inputs[9]);
                int extra2 = int.Parse(inputs[10]);

                switch(unitType)
                {
                    case 0:
                        units.Add(new Reaper(unitId, unitType, player, mass, radius, x, y, vx, vy, extra, extra2));
                        break;

                    case 1:
                        units.Add(new Destroyer(unitId, unitType, player, mass, radius, x, y, vx, vy, extra, extra2));
                        break;
                        
                    case 2:
                        units.Add(new Doof(unitId, unitType, player, mass, radius, x, y, vx, vy, extra, extra2));
                        break;
                        
                    case 3:
                        units.Add(new Tanker(unitId, unitType, player, mass, radius, x, y, vx, vy, extra, extra2));
                        break;
                        
                    case 4:
                        units.Add(new Wreck(unitId, unitType, player, mass, radius, x, y, vx, vy, extra, extra2));
                        break; 
                                           
                }

                if (player != -1)
                {
                joueurs[player].units.Add(units.Last());
                }

            }

            Joueur me = joueurs[0];
            foreach(Looter l in me.units)
            {
                l.Decide();
            }
            
            
            for (int i = 0 ; i < (3 - me.units.Count) ; i++ )
            {
            Console.WriteLine("WAIT");
            }


            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


        }
    }
}