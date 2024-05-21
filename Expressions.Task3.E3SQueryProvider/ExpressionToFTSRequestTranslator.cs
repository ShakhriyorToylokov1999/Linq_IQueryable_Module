using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        readonly StringBuilder _resultStringBuilder;

        public ExpressionToFtsRequestTranslator()
        {
            _resultStringBuilder = new StringBuilder();
        }

        public string Translate(Expression exp)
        {
            Visit(exp);

            return _resultStringBuilder.ToString();
        }

        #region protected methods

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }
            else if (node.Method.DeclaringType == typeof(string))
            {
                var constantStr = node.Arguments[0];
                var leftSideStr = node.Object;
                switch (node.Method.Name)
                {

                    case "Contains":


                        Visit(leftSideStr);
                        _resultStringBuilder.Append("(*");

                        Visit(constantStr);

                        _resultStringBuilder.Append("*)");
                        break;

                    case "Equals":
                        Visit(leftSideStr);
                        _resultStringBuilder.Append("(");

                        Visit(constantStr);

                        _resultStringBuilder.Append(")");
                        break;

                    case "EndsWith":
                        Visit(leftSideStr);
                        _resultStringBuilder.Append("(*");

                        Visit(constantStr);

                        _resultStringBuilder.Append(")");
                        break;

                    case "StartsWith":
                        Visit(leftSideStr);
                        _resultStringBuilder.Append("(");

                        Visit(constantStr);

                        _resultStringBuilder.Append("*)");
                        break;

                    default:
                        break;
                }



                return node;
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    Expression leftNode;
                    Expression rightNode;
                    if (node.Left.NodeType != ExpressionType.MemberAccess || node.Right.NodeType != ExpressionType.Constant)
                    {
                        leftNode = node.Right;
                        rightNode = node.Left;
                    }
                    else
                    {
                        leftNode = node.Left;
                        rightNode = node.Right;
                    }

                    Visit(leftNode);
                    _resultStringBuilder.Append("(");
                    Visit(rightNode);
                    _resultStringBuilder.Append(")");
                    break;
                
                case ExpressionType.AndAlso:
                    _resultStringBuilder.Append("{  \"query\":\" ");
                    Visit(node.Left);
                    _resultStringBuilder.Append("},");
                    
                    _resultStringBuilder.Append("{  \"query\":\" ");
                    Visit(node.Right);

                    _resultStringBuilder.Insert(0, "\"statements\": [");
                    _resultStringBuilder.Append("}]");
                    break;

                default:
                    throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _resultStringBuilder.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _resultStringBuilder.Append(node.Value);

            return node;
        }

        #endregion
    }
}
