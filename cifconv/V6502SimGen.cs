using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using ClipperLib;

namespace cifconv
{
	using Path = List<IntPoint>;
	using Paths = List<List<IntPoint>>;

	public class V6502SimGen
	{
		private const long touchDelta = 4;

		private Layout layout;

		public V6502SimGen(Layout layout)
		{
			this.layout = layout;
		}

		public void Generate(Stream s)
		{
			Dictionary<string, Paths> layers = new Dictionary<string, Paths>();
			foreach (string layer in new string[] { "metal1", "via1", "metal2", "active", "p-select", "n-select",
			                                        "poly", "contact", "n-well" })
			{
				if (!layout.Layers.ContainsKey(layer))
					continue;
				Console.Error.WriteLine("Simplify and unify polygons in " + layer + " layer...");
				Paths compl = new Paths();
				foreach (IDrawable d in layout.Layers[layer])
				{
					Path p = new Path();
					Polygon pol;
					if (d is Polygon)
					{
						pol = (Polygon)d;
					}
					else if (d is Box)
					{
						pol = ((Box)d).ToPolygon();
					}
					else
					{
						Console.Error.WriteLine("Warning: Can only handle boxes and polygons. (No rounds)");
						continue;
					}
					for (int i = 0; i < pol.P.Count; i++)
						p.Add(new IntPoint((long)pol.P[i].X, (long)pol.P[i].Y));
					if (!Clipper.Orientation(p))
						p.Reverse();
					compl.Add(p);
				}
				Paths sp = Clipper.SimplifyPolygons(compl, PolyFillType.pftNonZero);
				//sp = Clipper.CleanPolygons(sp);
				if (sp.Count > 0)
					layers.Add(layer, sp);
			}

			if (layers.ContainsKey("p-select") && layers.ContainsKey("active"))
			{
				Console.Error.WriteLine("Intersect active and p-select layers to get p-active layer...");
				Clipper c = new Clipper();
				c.AddPaths(layers["active"], PolyType.ptSubject, true);
				c.AddPaths(layers["p-select"], PolyType.ptClip, true);
				Paths p = new Paths();
				c.Execute(ClipType.ctIntersection, p, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				layers.Add("p-active", p);
			}

			if (layers.ContainsKey("n-select") && layers.ContainsKey("active"))
			{
				Console.Error.WriteLine("Intersect active and n-select layers to get n-active layer...");
				Clipper c = new Clipper();
				c.AddPaths(layers["active"], PolyType.ptSubject, true);
				c.AddPaths(layers["n-select"], PolyType.ptClip, true);
				Paths p = new Paths();
				c.Execute(ClipType.ctIntersection, p, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				layers.Add("n-active", p);
			}

			if (layers.ContainsKey("p-active") && layers.ContainsKey("poly"))
			{
				Console.Error.WriteLine("Intersect p-active and poly layers to get pmos layer...");
				Clipper c = new Clipper();
				c.AddPaths(layers["p-active"], PolyType.ptSubject, true);
				c.AddPaths(layers["poly"], PolyType.ptClip, true);
				Paths p = new Paths();
				c.Execute(ClipType.ctIntersection, p, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				layers.Add("pmos", p);
			}

			if (layers.ContainsKey("n-active") && layers.ContainsKey("poly"))
			{
				Console.Error.WriteLine("Intersect n-active and poly layers to get nmos layer...");
				Clipper c = new Clipper();
				c.AddPaths(layers["n-active"], PolyType.ptSubject, true);
				c.AddPaths(layers["poly"], PolyType.ptClip, true);
				Paths p = new Paths();
				c.Execute(ClipType.ctIntersection, p, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				layers.Add("nmos", p);
			}

			if (layers.ContainsKey("p-active") && layers.ContainsKey("poly"))
			{
				Console.Error.WriteLine("Cut p-active with poly layer to get p-active nets separated at transistors...");
				Clipper c = new Clipper();
				c.AddPaths(layers["p-active"], PolyType.ptSubject, true);
				c.AddPaths(layers["poly"], PolyType.ptClip, true);
				Paths p = new Paths();
				c.Execute(ClipType.ctDifference, p, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				layers["p-active"] = p;
			}

			if (layers.ContainsKey("n-active") && layers.ContainsKey("poly"))
			{
				Console.Error.WriteLine("Cut n-active with poly layer to get n-active nets separated at transistors...");
				Clipper c = new Clipper();
				c.AddPaths(layers["n-active"], PolyType.ptSubject, true);
				c.AddPaths(layers["poly"], PolyType.ptClip, true);
				Paths p = new Paths();
				c.Execute(ClipType.ctDifference, p, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				layers["n-active"] = p;
			}

			if (layers.ContainsKey("n-well") && layers.ContainsKey("pmos"))
			{
				Console.Error.WriteLine("Subtract n-well from pmos layer to check for misplaced PMOS...");
				Clipper c = new Clipper();
				c.AddPaths(layers["pmos"], PolyType.ptSubject, true);
				c.AddPaths(layers["n-well"], PolyType.ptClip, true);
				Paths p = new Paths();
				c.Execute(ClipType.ctDifference, p, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				if (p.Count > 0)
					Console.Error.WriteLine("Warning: Found stray PMOS transistors that escaped their well.");
			}

			if (layers.ContainsKey("n-well") && layers.ContainsKey("nmos"))
			{
				Console.Error.WriteLine("Intersect n-well with nmos layer to check for misplaced NMOS...");
				Clipper c = new Clipper();
				c.AddPaths(layers["nmos"], PolyType.ptSubject, true);
				c.AddPaths(layers["n-well"], PolyType.ptClip, true);
				Paths p = new Paths();
				c.Execute(ClipType.ctIntersection, p, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				if (p.Count > 0)
					Console.Error.WriteLine("Warning: Found stray NMOS transistors that sneaked into a well.");
			}

			List<Paths> pmos = new List<Paths>();
			List<Paths> nmos = new List<Paths>();
			List<Paths> pact = new List<Paths>();
			List<Paths> nact = new List<Paths>();
			List<Paths> poly = new List<Paths>();
			List<Paths> cont = new List<Paths>();
			List<Paths> met1 = new List<Paths>();
			List<Paths> met2 = new List<Paths>();
			List<Paths> via1 = new List<Paths>();

			foreach (var kvp in new Dictionary<string, List<Paths>> {
			                        { "pmos",     pmos },
			                        { "nmos",     nmos },
			                        { "p-active", pact },
			                        { "n-active", nact },
			                        { "poly",     poly },
			                        { "contact",  cont },
			                        { "metal1",   met1 },
			                        { "metal2",   met2 },
			                        { "via1",     via1 }})
			{
				if (!layers.ContainsKey(kvp.Key))
					continue;
				Console.Error.WriteLine("Create segment list for layer " + kvp.Key + "...");
				Clipper c = new Clipper();
				c.AddPaths(layers[kvp.Key], PolyType.ptSubject, true);
				PolyTree pt = new PolyTree();
				c.Execute(ClipType.ctUnion, pt, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				PolyTreeToList(kvp.Value, pt);
			}

			Dictionary<string, Transistor> trans = new Dictionary<string, Transistor>();
			Dictionary<Paths, Segment> segs = new Dictionary<Paths, Segment>();

			int cnt = 0;
			foreach (var p in nmos)
			{
				Transistor t = new Transistor();
				t.Name = "n" + cnt.ToString(CultureInfo.InvariantCulture);
				t.IsPmos = false;
				t.Paths = p;
				trans.Add(t.Name, t);
				cnt++;
			}

			cnt = 0;
			foreach (var p in pmos)
			{
				Transistor t = new Transistor();
				t.Name = "p" + cnt.ToString(CultureInfo.InvariantCulture);
				t.IsPmos = true;
				t.Paths = p;
				trans.Add(t.Name, t);
				cnt++;
			}

			foreach (var p in nact)
			{
				Segment seg = new Segment();
				seg.Layer = NDIFF;
				seg.Paths = p;
				segs.Add(p, seg);
			}

			foreach (var p in pact)
			{
				Segment seg = new Segment();
				seg.Layer = PDIFF;
				seg.Paths = p;
				segs.Add(p, seg);
			}

			foreach (var p in poly)
			{
				Segment seg = new Segment();
				seg.Layer = POLY;
				seg.Paths = p;
				segs.Add(p, seg);
			}

			foreach (var p in met1)
			{
				Segment seg = new Segment();
				seg.Layer = METAL1;
				seg.Paths = p;
				segs.Add(p, seg);
			}

			foreach (var p in met2)
			{
				Segment seg = new Segment();
				seg.Layer = METAL2;
				seg.Paths = p;
				segs.Add(p, seg);
			}

			foreach (var p in cont)
			{
				Segment seg = new Segment();
				seg.Layer = CONTACT;
				seg.Paths = p;
				segs.Add(p, seg);
			}

			foreach (var p in via1)
			{
				Segment seg = new Segment();
				seg.Layer = VIA1;
				seg.Paths = p;
				segs.Add(p, seg);
			}

			List<Segment> strays = new List<Segment>();

			Console.Error.WriteLine("Find active and poly segments connected to transistor terminals...");
			foreach (var t in trans.Values)
			{
				var co = new ClipperOffset();
				co.AddPaths(t.Paths, JoinType.jtSquare, EndType.etClosedPolygon);
				Paths infl = new Paths();
				co.Execute(ref infl, touchDelta);
				List<Paths> gi = FindIntersections(infl, poly);
				List<Paths> ci = FindIntersections(infl, t.IsPmos ? pact : nact);
				if (gi.Count != 1)
					Console.Error.WriteLine("Error: Found transistor with " + gi.Count.ToString() + " gate connections.");
				if (ci.Count != 2)
					Console.Error.WriteLine("Error: Found transistor with " + ci.Count.ToString() + " terminal connections.");
				if (gi.Count != 1 || ci.Count != 2)
					throw new ApplicationException("Wrong connection count to transistor. Try increasing touchDelta or increasing scale.");
				t.Gate = segs[gi[0]];
				t.C1 = segs[ci[0]];
				t.C2 = segs[ci[1]];
				t.Gate.Gates.Add(t);
				t.C1.C1C2s.Add(t);
				t.C2.C1C2s.Add(t);
			}

			List<SegmentGroup> groups = new List<SegmentGroup>();

			Console.Error.WriteLine("Find contacts connected to metal1...");
			foreach (var m in met1)
			{
				Segment mseg = segs[m];
				SegmentGroup g = new SegmentGroup();
				g.Segments.Add(mseg);
				mseg.Group = g;
				groups.Add(g);
				List<Segment> csegs = FindIntersectionsWithGroupless(m, cont, segs);
				foreach (var cseg in csegs)
				{
					g.Segments.Add(cseg);
					cseg.Group = g;
				}
			}

			Console.Error.WriteLine("Find contacts connected to poly or active...");
			foreach (var c in cont)
			{
				Segment cseg = segs[c];
				SegmentGroup g = cseg.Group;
				if (g == null)
				{
					Console.Error.WriteLine("Warning: Found contact not connected to metal1.");
					strays.Add(cseg);
					continue;
				}
				List<Segment> psegs = FindIntersectionsWithOtherGroups(c, poly, segs, g);
				foreach (var pseg in psegs)
				{
					if (pseg.Group != null)
					{
						g = MergeGroups(groups, g, pseg.Group);
					}
					else
					{
						g.Segments.Add(pseg);
						pseg.Group = g;
					}
				}
				if (psegs.Count > 0)
					continue;
				List<Segment> pasegs = FindIntersectionsWithGroupless(c, pact, segs);
				foreach (var paseg in pasegs)
				{
					if (paseg.Group != null)
					{
						g = MergeGroups(groups, g, paseg.Group);
					}
					else
					{
						g.Segments.Add(paseg);
						paseg.Group = g;
					}
				}
				if (pasegs.Count > 0)
					continue;
				List<Segment> nasegs = FindIntersectionsWithGroupless(c, nact, segs);
				foreach (var naseg in nasegs)
				{
					if (naseg.Group != null)
					{
						g = MergeGroups(groups, g, naseg.Group);
					}
					else
					{
						g.Segments.Add(naseg);
						naseg.Group = g;
					}
				}
			}

			Console.Error.WriteLine("Find poly segments without contact to metal1...");
			foreach (var p in poly)
			{
				Segment pseg = segs[p];
				SegmentGroup g = pseg.Group;
				if (g != null)
					continue;
				g = new SegmentGroup();
				g.Segments.Add(pseg);
				pseg.Group = g;
				if (pseg.Gates.Count == 0)
				{
					Console.Error.WriteLine("Warning: Found poly segment w/o connection to metal1 or transistor.");
					strays.Add(pseg);
				}
			}

			Console.Error.WriteLine("Find p-active segments without contact to metal1...");
			foreach (var pa in pact)
			{
				Segment paseg = segs[pa];
				SegmentGroup g = paseg.Group;
				if (g != null)
					continue;
				g = new SegmentGroup();
				g.Segments.Add(paseg);
				paseg.Group = g;
				if (paseg.C1C2s.Count == 0)
				{
					Console.Error.WriteLine("Warning: Found p-active segment w/o connection to metal1 or transistor.");
					strays.Add(paseg);
				}
			}

			Console.Error.WriteLine("Find n-active segments without contact to metal1...");
			foreach (var na in nact)
			{
				Segment naseg = segs[na];
				SegmentGroup g = naseg.Group;
				if (g != null)
					continue;
				g = new SegmentGroup();
				g.Segments.Add(naseg);
				naseg.Group = g;
				if (naseg.C1C2s.Count == 0)
				{
					Console.Error.WriteLine("Warning: Found n-active segment w/o connection to metal1 or transistor.");
					strays.Add(naseg);
				}
			}

			Console.Error.WriteLine("Find via1 connected to metal1...");
			foreach (var m in met1)
			{
				Segment mseg = segs[m];
				List<Segment> vsegs = FindIntersectionsWithGroupless(m, via1, segs);
				foreach (var vseg in vsegs)
				{
					SegmentGroup g = mseg.Group;
					g.Segments.Add(vseg);
					vseg.Group = g;
				}
			}

			Console.Error.WriteLine("Find via1 connected to metal2...");
			foreach (var v in via1)
			{
				Segment vseg = segs[v];
				SegmentGroup g = vseg.Group;
				if (g == null)
				{
					Console.Error.WriteLine("Warning: Found via1 not connected to metal1.");
					strays.Add(vseg);
					continue;
				}
				List<Segment> msegs = FindIntersectionsWithOtherGroups(v, met2, segs, g);
				foreach (var mseg in msegs)
				{
					if (mseg.Group != null)
					{
						g = MergeGroups(groups, g, mseg.Group);
					}
					else
					{
						g.Segments.Add(mseg);
						mseg.Group = g;
					}
				}
			}

			for (int i = 0; i < groups.Count; i++)
				groups[i].Id = i;

			StreamWriter w = new StreamWriter(s);
			w.WriteLine("{");
			w.Write("\"segments\": [");
			string sep = "";
			List<Segment> segsa = new List<Segment>();
			int k = 0;
			foreach (var seg in segs.Values)
			{
				Paths b = new Paths();
				Paths h = new Paths();
				b.Add(seg.Paths[0]);
				for (int j = 1; j < seg.Paths.Count; j++)
					h.Add(seg.Paths[j]);
				w.WriteLine(sep);
				w.WriteLine("{");
				w.WriteLine("\"boundary\": ");
				PrintPolys(w, b);
				w.WriteLine(",");
				w.WriteLine("\"holes\": [");
				PrintPolys(w, h);
				w.WriteLine("]");
				w.WriteLine("}");
				sep = ",";
				segsa.Add(seg);
				k++;
			}
			w.WriteLine();
			w.WriteLine("],");
			w.Write("\"stray_segment_indices\": [");
			sep = "";
			for (int i = 0; i < strays.Count; i++)
			{
				w.Write(sep);
				w.Write(segsa.IndexOf(strays[i]).ToString());
				sep = ", ";
			}
			w.WriteLine("],");
			w.WriteLine("\"pixels_per_unit\": 1.0");
			w.WriteLine("}");
			w.Flush();
		}

		private static SegmentGroup MergeGroups(List<SegmentGroup> groups, SegmentGroup a, SegmentGroup b)
		{
			if (a == b)
				return a;
			SegmentGroup keep = a;
			SegmentGroup drop = b;
			if (a.Segments.Count < b.Segments.Count)
			{
				keep = b;
				drop = a;
			}
			foreach (var seg in drop.Segments)
			{
				keep.Segments.Add(seg);
				seg.Group = keep;
			}
			groups.Remove(drop);
			return keep;
		}

		private static List<Paths> FindIntersections(Paths p, List<Paths> l)
		{
			List<Paths> r = new List<Paths>();
			foreach (var s in l)
			{
				Clipper c = new Clipper();
				c.AddPaths(p, PolyType.ptSubject, true);
				c.AddPaths(s, PolyType.ptClip, true);
				Paths i = new Paths();
				c.Execute(ClipType.ctIntersection, i, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				if (i.Count > 0)
					r.Add(s);
			}
			return r;
		}

		private static List<Segment> FindIntersectionsWithGroupless(Paths p, List<Paths> l,
		                                                            Dictionary<Paths, Segment> segs)
		{
			List<Segment> r = new List<Segment>();
			foreach (var s in l)
			{
				Segment seg = segs[s];
				if (seg.Group != null)
					continue;
				Clipper c = new Clipper();
				c.AddPaths(p, PolyType.ptSubject, true);
				c.AddPaths(s, PolyType.ptClip, true);
				Paths i = new Paths();
				c.Execute(ClipType.ctIntersection, i, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				if (i.Count > 0)
					r.Add(seg);
			}
			return r;
		}

		private static List<Segment> FindIntersectionsWithOtherGroups(Paths p, List<Paths> l,
		                                                              Dictionary<Paths, Segment> segs,
		                                                              SegmentGroup g)
		{
			List<Segment> r = new List<Segment>();
			foreach (var s in l)
			{
				Segment seg = segs[s];
				if (seg.Group == g)
					continue;
				Clipper c = new Clipper();
				c.AddPaths(p, PolyType.ptSubject, true);
				c.AddPaths(s, PolyType.ptClip, true);
				Paths i = new Paths();
				c.Execute(ClipType.ctIntersection, i, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				if (i.Count > 0)
					r.Add(seg);
			}
			return r;
		}

		private class Transistor
		{
			public string  Name;
			public bool    IsPmos;
			public Paths   Paths;
			public Segment Gate;
			public Segment C1;
			public Segment C2;
		}

		private const int METAL1  = 0;
		private const int NDIFF   = 1;
		private const int PDIFF   = 2;
		private const int GNDDIFF = 3;
		private const int PWDDIFF = 4;
		private const int POLY    = 5;
		private const int METAL2  = 6;

		private const int CONTACT = -1;
		private const int VIA1    = -2;

		private class Segment
		{
			public SegmentGroup     Group;
			public int              Layer;
			public Paths            Paths;
			public List<Transistor> Gates = new List<Transistor>();
			public List<Transistor> C1C2s = new List<Transistor>();
		}

		private class SegmentGroup
		{
			public int           Id;
			public List<Segment> Segments = new List<Segment>();
		}

		private static void PolyTreeToList(List<Paths> l, PolyNode p)
		{
			foreach (var c in p.Childs)
			{
				if (c.IsHole)
				{
					Console.Error.WriteLine("Warning: Found hole in poly tree that should be an outer boundary...");
					continue;
				}
				Paths il = new Paths();
				il.Add(c.Contour);
				if (c.ChildCount > 0)
					Console.Error.WriteLine("Info: Found polygon with hole(s)...");
				foreach (var h in c.Childs)
				{
					if (!h.IsHole)
					{
						Console.Error.WriteLine("Warning: Found island in poly tree that should be a hole...");
						PolyTreeToList(l, h);
						continue;
					}
					il.Add(h.Contour);
					if (h.ChildCount > 0)
						Console.Error.WriteLine("Info: Found hole with island(s)...");
					foreach (var i in h.Childs)
						PolyTreeToList(l, i);
				}
				l.Add(il);
			}
		}

		private static void PrintPolys(StreamWriter w, Paths p)
		{
			string sep = "";
			foreach (var x in p)
			{
				w.Write(sep);
				w.Write("[");
				string sep2 = "";
				foreach (var y in x)
				{
					w.Write(sep2);
					w.Write("[" + y.X.ToString() + ", " + y.Y.ToString() + "]");
					sep2 = ", ";
				}
				w.Write("]");
				sep = ", ";
			}
		}
	}
}
