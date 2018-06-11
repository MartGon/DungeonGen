using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PerformanceController{

    public struct DungeonPerformanceReport
    {
        public float rooms;
        public float corridors;
        public float block;
        public float misc;
    }

    private static List<DungeonPerformanceReport> reports = new List<DungeonPerformanceReport>();

    public static void addReport(DungeonPerformanceReport report)
    {
        reports.Add(report);
    }

    public static int getReportsCount()
    {
        return reports.Count;
    }

    public static DungeonPerformanceReport getReportByIndex(int index)
    {
        if (index >= reports.Count || index < 0)
            return new DungeonPerformanceReport();

        return reports[index];
    }
}
