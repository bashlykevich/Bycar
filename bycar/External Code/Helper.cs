using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bycar3.External_Code;

namespace bycar.External_Code
{
    public static class Helper
    {
        public static string CalculateQRests(int SpareID)
        {
            SpareView spare = SpareContainer.Instance.GetSpare(SpareID);
            DataAccess da = new DataAccess();
            string xml = "";
            List<warehouse> warehouses = da.GetWarehouses();
            List<double> Qs = new List<double>();
            xml += "<r>";
            // Q total
            double q0 = spare.QRest.HasValue? spare.QRest.Value:0;
            Qs.Add(q0);

            // all actual incomes
            List<SpareInSpareIncomeView> incomes = da.GetActualIncomes();

            // Q by warehouses
            foreach (warehouse w in warehouses)
            {
                // incomes by warehouse
                decimal? q = incomes.Where(i => (i.WarehouseID == w.id && i.SpareID == SpareID)).Sum(i => i.QRest);
                if (q.HasValue)
                    Qs.Add((double)q.Value);
                else
                    Qs.Add(0);
            }

            foreach (double q in Qs)
                xml += "<w q=\"" + q.ToString() + "\"/>";
            xml += @"</r>";

            // save sml to db
            da.SpareEdit(SpareID, xml);
            return xml;
        }
    }
}
