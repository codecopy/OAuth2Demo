using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAuth2DemoClientManage
{
    public class OPResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSucceed { get; set; }

        /// <summary>
        /// 操作的结果说明
        /// </summary>
        public string Message { get; set; }
    }

    public class OPResult<TResult> : OPResult
    {
        public TResult Result { get; set; }
    }

    public class JTableResult<TData>
    {
        public string Result { get; set; }

        public TData Record { get; set; }

        public IEnumerable<TData> Records { get; set; }

        public string Message { get; set; }
    }
}