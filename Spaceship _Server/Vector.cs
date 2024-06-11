using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spaceship__Server
{
    public class Vector

    {
        public List<int> coords { get; set; } = new List<int>();

        public Vector(List<int> lis)

        {

            for (int i = 0; i < lis.Count; i++)

            {

                coords.Add(lis[i]);

            }

        }

        public Vector(params int[] args)

        {

            for (int i = 0; i < args.Length; i++)

            {

                coords.Add(args[i]);

            }

        }

        public override string ToString()

        {

            string str = "";

            str += "Vector(";

            for (int i = 0; i < coords.Count; i++)

            {

                if (i != coords.Count - 1)

                {

                    str += coords[i] + ", ";

                }

                else

                {

                    str += coords[i];

                }

            }

            str += ")";

            return str;

        }

#nullable enable
        public override bool Equals(object? obj)
        {
            if (!(obj is Vector vector))
            {
                return false;
            }

            if (vector.coords.Count != this.coords.Count)
            {
                return false;
            }

            for (int i = 0; i < this.coords.Count; i++)
            {
                if (this.coords[i] != vector.coords[i])
                {
                    return false;
                }
            }
            return true;

        }
#nullable disable
        public override int GetHashCode()
        {
            return HashCode.Combine(coords, this.coords.Count);
        }

        public int this[int index]

        {

            get

            {
                if (index <= this.coords.Count)
                {
                    return coords[index];
                }
                else
                {
                    throw new ArgumentException();
                }

            }
            set

            {
                if (index <= this.coords.Count)
                {
                    coords[index] = value;
                }
                else
                {
                    throw new ArgumentException();
                }

            }

        }

        public static Vector operator +(Vector a, Vector b)

        {

            if (a.coords.Count != b.coords.Count)

            {

                throw new ArgumentException();

            }

            else

            {

                List<int> ansvec = new List<int>();

                for (int i = 0; i < a.coords.Count; i++)

                {

                    ansvec.Add(a[i] + b[i]);

                }

                Vector ans = new Vector(ansvec);

                return ans;

            }

        }

        public static Vector operator -(Vector a, Vector b)

        {

            if (a.coords.Count != b.coords.Count)

            {

                throw new ArgumentException();

            }

            else

            {

                List<int> ansvec = new List<int>();

                for (int i = 0; i < a.coords.Count; i++)

                {

                    ansvec.Add(a[i] - b[i]);

                }

                Vector ans = new Vector(ansvec);

                return ans;

            }

        }

        public static bool operator ==(Vector a, Vector b)

        {

            int size = Math.Max(a.coords.Count, b.coords.Count);

            if (a.coords.Count != b.coords.Count)

            {

                return false;

            }

            for (int i = 0; i < size; i++)

            {

                if (a[i] != b[i])

                {

                    return false;

                }

            }

            return true;

        }

        public static bool operator !=(Vector a, Vector b)

        {

            if (a == b)

            {

                return false;

            }

            else

            {

                return true;

            }

        }
    }
}
