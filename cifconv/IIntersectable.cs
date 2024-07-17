namespace cifconv
{
	public interface IIntersectable
	{
		Box GetAABB();
		bool Contains(Vector v);
		bool Intersects(Box o);
		bool Intersects(Round r);
		bool Intersects(Polygon p);
		bool Intersects(IDrawable d);
		bool SegmentIntersects(Line l);
	}
}
