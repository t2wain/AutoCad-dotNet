using RacewayDataLib;

namespace TestProject
{
    public class ReadDataTest : IClassFixture<Context>
    {
        private Context _ctx;

        public ReadDataTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void ReadAllData()
        {
            var data = NetworkDB.LoadData(_ctx.FileConfig);
            Assert.True(data.Raceways.Count() > 0);
            Assert.True(data.Raceways.Where(r => r.FromNode == null).Count() == 0);
            Assert.True(data.Raceways.Where(r => r.ToNode == null).Count() == 0);
        }

        [Fact]
        public void ReadRaceway()
        {
            var lstRw = NetworkDB.ReadRaceways(_ctx.FileConfig.RacewayFile);
            Assert.True(lstRw.Count() > 0);
        }

        [Fact]
        public void ReadNode()
        {
            var lstNode = NetworkDB.ReadNodes(_ctx.FileConfig.NodeFile);
            Assert.True(lstNode.Count() > 0);
        }

        [Fact]
        public void ReadSegSystem()
        {
            var lstSys = NetworkDB.ReadSegSystems(_ctx.FileConfig.SegSystemFile);
            Assert.True(lstSys.Count() > 0);
        }

        [Fact]
        public void ReadCable()
        {
            var lstCable = NetworkDB.ReadCables(_ctx.FileConfig.CableFile);
            Assert.True(lstCable.Count() > 0);
        }

        [Fact]
        public void ReadRoute()
        {
            var lstRoute = NetworkDB.ReadRoutes(_ctx.FileConfig.RouteFile);
            Assert.True(lstRoute.Count() > 0);
        }

        [Fact]
        public void GetCableNode()
        {
            var lstNode = NetworkDB.ReadNodes(_ctx.FileConfig.NodeFile);
            var lstCable = NetworkDB.ReadCables(_ctx.FileConfig.CableFile);
            Assert.True(lstNode.Count() > 0);
        }

    }
}