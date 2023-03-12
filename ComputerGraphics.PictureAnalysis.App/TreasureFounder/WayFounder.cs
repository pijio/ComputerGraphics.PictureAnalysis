using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ComputerGraphics.PictureAnalysis.App.Areas;

namespace ComputerGraphics.PictureAnalysis.App.TreasureFounder
{
    public class WayFounder
    {
        /// <summary>
        /// Поиск пути от начала до клада
        /// </summary>
        /// <param name="areas"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Way FoundWay(Dictionary<int, LinkedList<int[]>> areas, Bitmap image)
        {
            var wayArrows = new LinkedList<WayArrow>();

            foreach (var kvp in areas)
            {
                var res = WayArrow.GetOnValidate(kvp.Value, (Bitmap)image);
                if (res == null)
                    continue;
                wayArrows.AddLast(res);
            }

            var startArrows = new LinkedList<StartArrow>();

            foreach (var kvp in areas)
            {
                var res = StartArrow.GetOnValidate(kvp.Value, (Bitmap)image);
                if (res == null)
                    continue;
                startArrows.AddLast(res);
            }

            var centersOfMass = AreaAnalyzer.CentersOfMass(areas);

            var possibleTreasures = centersOfMass.AsParallel().Where(z =>
                    !wayArrows.Any(o => o.CenterOfMass.X == z.Item2 && o.CenterOfMass.Y == z.Item3)).Where(v =>
                    !startArrows.Any(eb => eb.CenterOfMass.X == v.Item2 && eb.CenterOfMass.Y == v.Item3))
                .Select(at => new TreasureArea(at.Item2, at.Item3)).ToList();

            var current = (WayArrow)startArrows.First.Value;
            var treasureFound = false;
            var way = new LinkedList<WayArrow>();
            TreasureArea end = null;
            way.AddLast(current);
            while (!treasureFound)
            {
                if (wayArrows.Count != 0)
                {
                    WayArrow closest = null;
                    var minDistance = double.MaxValue;
                    foreach (var arrow in wayArrows)
                    {
                        var distanceToCurrent = Utilities.DistanceToPoint(current.CenterOfMass, arrow.CenterOfMass);

                        if (distanceToCurrent < minDistance && Utilities.LocateOnLine(current.CenterOfMass, arrow.CenterOfMass, current.Angle))
                        {
                            closest = arrow;
                            minDistance = distanceToCurrent;
                        }
                    }

                    if (closest == null)
                        closest = way.Last.Value;

                    TreasureArea closestTreausre = null;
                    var minDistanceT = double.MaxValue;
                    foreach (var treasure in possibleTreasures)
                    {
                        var distanceToArrow =
                            Utilities.DistanceToPoint(closest.CenterOfMass, treasure.CenterOfMass);

                        if (distanceToArrow < minDistanceT && Utilities.LocateOnLine(way.Last.Value.CenterOfMass, treasure.CenterOfMass, way.Last.Value.Angle))
                        {
                            closestTreausre = treasure;
                            minDistanceT = distanceToArrow;
                        }
                    }

                    if (minDistance > minDistanceT)
                    {
                        end = closestTreausre;
                        treasureFound = true;
                    }
                    else
                    {
                        wayArrows.Remove(closest);
                        way.AddLast(closest);
                        current = closest;
                    }
                }

                else
                {
                    TreasureArea closestTreausre = null;
                    var minDistanceT = double.MaxValue;
                    foreach (var treasure in possibleTreasures)
                    {
                        var lastArrow = way.Last.Value;
                        var distanceToArrow =
                            Utilities.DistanceToPoint(lastArrow.CenterOfMass, treasure.CenterOfMass);

                        if (distanceToArrow < minDistanceT && Utilities.LocateOnLine(lastArrow.CenterOfMass, treasure.CenterOfMass, lastArrow.Angle))
                        {
                            closestTreausre = treasure;
                            minDistanceT = distanceToArrow;
                        }
                    }

                    if (closestTreausre != null)
                    {
                        end = closestTreausre;
                        treasureFound = true;
                    }
                    else
                        throw new InvalidOperationException("Не удалось найти путь!");
                }

            }
            return new Way() { End = end, WayArrows = way };
        }
    }
}