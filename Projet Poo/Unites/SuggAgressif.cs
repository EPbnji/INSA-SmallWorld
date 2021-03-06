﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibCLR;

namespace Modele
{
    public class SuggAgressif : Suggestion
    {
        Carte carte;
        Unite unite;
        List<Node> closedset = new List<Node>();
        List<Node> openset = new List<Node>();
        Dictionary<Node, Node> came_from = new Dictionary<Node, Node>();
        /// <summary>
        /// Récupère les suggestions aggressive. Ces suggestions sont fait en sorte de déplacer
        /// l'unité vers une autre unité le plus proche avec l'algorithme A*
        /// </summary>
        /// <param name="carte">La carte sur laquel l'unité se trouve</param>
        /// <param name="unite">L'unité à déplacer</param>
        /// <returns></returns>
        public override SuggMap getSuggestion(Carte carte, Unite unite)
        {
            this.carte = carte;
            this.unite = unite;
            
            //on récupère les suggestions classique en fonction du terrain
            SuggMap res = base.getSuggestion(carte, unite);

            
            List<Coordonnees> listCoordAttaquableProche = new List<Coordonnees>();
            List<Coordonnees> listCoordAttaquable = new List<Coordonnees>();
            // cherche les unités a proximité, et les autres
            foreach (Unite u in carte.Unites.Where(z => z.IdProprietaire!=unite.IdProprietaire))
            {
                if (res[u.Coord].Sugg != 0)
                {
                    if (!listCoordAttaquableProche.Contains(u.Coord))
                        listCoordAttaquableProche.Add(u.Coord);
                }
                else
                {
                    if (!listCoordAttaquable.Contains(u.Coord))
                        listCoordAttaquable.Add(u.Coord);
                }
            }
            if (listCoordAttaquableProche.Count > 0) // si il y a des unités a proximité, on l'attaque
            {
                attaqueUniteProche(carte, res, listCoordAttaquableProche);
            }
            else if (listCoordAttaquable.Count>0) // sinon on se déplace vers les plus proche
            {
                Coordonnees plusProche = unite.Coord.getClosiestCoord(listCoordAttaquable);

                pathFinding(carte, unite, res, plusProche);
            }

            return res;
        }
        /// <summary>
        /// Place des coefficients de suggestions élevés sur les coordonnees de la liste listCoordAttaquableProche
        /// </summary>
        /// <param name="carte"></param>
        /// <param name="res"></param>
        /// <param name="listCoordAttaquableProche"></param>
        private static void attaqueUniteProche(Carte carte, SuggMap res, List<Coordonnees> listCoordAttaquableProche)
        {
            Coordonnees coordAttable = null;
            int min = int.MaxValue;
            foreach (Coordonnees coord in listCoordAttaquableProche)
            {
                List<Unite> tmp = carte.getUniteFromCoord(coord);
                if (tmp.Count < min)
                {
                    coordAttable = coord;
                    min = tmp.Count;
                }
            }

            res[coordAttable].Sugg = int.MaxValue;
        }

        private void pathFinding(Carte carte, Unite unite, SuggMap res, Coordonnees plusProche)
        {
            int peuple = -1;
            IPeuple p = unite.Proprietaire.Peuple;

            if (p is PeupleViking)
                peuple = 0;
            else if (p is PeupleNain)
                peuple = 1;
            else if (p is PeupleGaulois)
                peuple = 2;

            WrapperAStar aStart = new WrapperAStar();
            List<Coordonnees> path = convertListInttoListCoord(aStart.pathFinding(carte.toList(), peuple, carte.Largeur, carte.Hauteur, unite.Coord.X, unite.Coord.Y, plusProche.X, plusProche.Y));

            if (path != null && path.Count > 0)
            {
                foreach (Coordonnees coord in path)
                {
                    if (res[coord].Sugg != 0)
                        res[coord].Sugg = (unite.PointsDepl-res[coord].Depl) + 10;
                }
            }
        }

        private List<Coordonnees> convertListInttoListCoord(List<int> path)
        {
            List<Coordonnees> res = new List<Coordonnees>();
            for (int i = 0; i < path.Count; i += 2)
            {
                res.Add(new Coordonnees(path[i],path[i+1]));
            }
            return res;
        }

        private List<Node> pathFinding(Node start, Node goal)
        {
            closedset.Clear();
            openset.Clear();
            came_from.Clear();

            openset.Add(start);
            start.G_score = 0;
            start.F_score = start.G_score + start.distance(goal);
            Node current=null;
            while (openset.Count>0)
            {
                openset.Sort(new ByFScore());
                current = openset.First();

                if (current == goal)
                    return reconstruct_path(came_from, goal);

                openset.Remove(current);
                closedset.Add(current);
                foreach (Node neighbor in neighbor_nodes(current))
                {
                    double tentative_g_score = current.G_score + current.distance(neighbor);
                    double tentative_f_score = tentative_g_score + neighbor.distance(goal);

                    if (closedset.Contains(neighbor) && tentative_f_score >= neighbor.F_score)
                        continue;

                    if (!openset.Contains(neighbor) || tentative_f_score < neighbor.F_score)
                    {
                        came_from[neighbor] = current;
                        neighbor.G_score = tentative_g_score;
                        neighbor.F_score = tentative_f_score;
                        if (!openset.Contains(neighbor))
                        {
                            openset.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private List<Node> reconstruct_path(Dictionary<Node, Node> came_from, Node current)
        {
            if (came_from.Keys.Contains(current))
            {
                List<Node> tmp = reconstruct_path(came_from,came_from[current]);
                tmp.Add(current);
                return tmp;
            }
            else
            {
                List<Node> path = new List<Node>();
                path.Add(current);
                return path;
            }
        }

        private List<Node> neighbor_nodes(Node current)
        {
            List<Node> res =  new List<Node>();
            Coordonnees coordCurrent = current.Coord;
            Node tmp;
            if (coordCurrent.X - 1 >= 0 && carte.Cases[coordCurrent.X-1][coordCurrent.Y].estAccessible(unite.Proprietaire.Peuple))
            {
                tmp = new Node(new Coordonnees(coordCurrent.X - 1, coordCurrent.Y));
                List<Node> tmp2 = closedset.Where(u => u == tmp).ToList();
                if (tmp2.Count>0)
                {
                    res.Add(tmp2.First());
                }
                else
                {
                    res.Add(tmp);
                }
                
            }
            if (coordCurrent.X + 1 < carte.Largeur && carte.Cases[coordCurrent.X + 1][coordCurrent.Y].estAccessible(unite.Proprietaire.Peuple))
            {
                tmp = new Node(new Coordonnees(coordCurrent.X + 1, coordCurrent.Y));
                List<Node> tmp2 = closedset.Where(u => u == tmp).ToList();
                if (tmp2.Count > 0)
                {
                    res.Add(tmp2.First());
                }
                else
                {
                    res.Add(tmp);
                }
            }
            if (coordCurrent.Y - 1 >= 0 && carte.Cases[coordCurrent.X][coordCurrent.Y-1].estAccessible(unite.Proprietaire.Peuple))
            {
                tmp = new Node(new Coordonnees(coordCurrent.X, coordCurrent.Y-1));
                List<Node> tmp2 = closedset.Where(u => u == tmp).ToList();
                if (tmp2.Count > 0)
                {
                    res.Add(tmp2.First());
                }
                else
                {
                    res.Add(tmp);
                }
            }
            if (coordCurrent.Y + 1 < carte.Hauteur && carte.Cases[coordCurrent.X][coordCurrent.Y+1].estAccessible(unite.Proprietaire.Peuple))
            {
                tmp = new Node(new Coordonnees(coordCurrent.X, coordCurrent.Y+1));
                List<Node> tmp2 = closedset.Where(u => u == tmp).ToList();
                if (tmp2.Count > 0)
                {
                    res.Add(tmp2.First());
                }
                else
                {
                    res.Add(tmp);
                }
            }

            return res;
        }

        private class Node : IComparable<Node>, IEquatable<Node>
        {
            public Node(Coordonnees coord)
            {
                this.coord = coord;
                g_score = 0;
                f_score = 0;
            }

            Coordonnees coord;

            public Coordonnees Coord
            {
                get { return coord; }
                set { coord = value; }
            }
            double g_score;

            public double G_score
            {
                get { return g_score; }
                set { g_score = value; }
            }
            double f_score;

            public double F_score
            {
                get { return f_score; }
                set { f_score = value; }
            }

            public static bool operator ==(Node a, Node b)
            {
                return (a.Coord == b.Coord);
            }

            public static bool operator !=(Node a, Node b)
            {
                return (a.Coord != b.Coord);
            }

            public override int GetHashCode()
            {
                return coord.GetHashCode();
            }

            public int CompareTo(Node other)
            {
                return ((this.coord.X - other.coord.X) + (this.coord.Y - other.coord.Y));
            }

            public override bool Equals(object obj)
            {
                Node other = obj as Node;
                return this.coord == other.coord;
            }

            public bool Equals(Node other)
            {
                return this.coord == other.coord;
            }

            public double distance(Node b)
            {
                return coord.distance(b.coord);
            }
        }

        private class ByFScore : IComparer<Node>
        {

            public int Compare(Node x, Node y)
            {
                return (int) (x.F_score - y.F_score);
            }
        }
    }
}
