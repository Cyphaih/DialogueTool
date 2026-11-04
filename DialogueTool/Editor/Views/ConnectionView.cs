using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;


    public class ConnectionView : VisualElement
    {
        public PortView output { get; private set; }
        public PortView input { get; private set; }
        
        public bool IsSelected { get; private set; }

        private const float k_LineWidth = 3.0f;
        private const float k_SelectionWidth = 10.0f; // Wider area for clicking

        public ConnectionView(PortView _output, PortView _input)
        {
            this.output = _output;
            this.input = _input;

            pickingMode = PickingMode.Ignore;
            
            
            usageHints = UsageHints.None;
            
            generateVisualContent += OnGenerateVisualContent;
            
            
        }

        public void Clearport()
        {
            input = null;
            output = null;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (output == null || input == null) return;

            var painter = mgc.painter2D;
            painter.lineWidth = k_LineWidth;
            painter.strokeColor = IsSelected ? Color.cyan : Color.white;

            Vector2 startPos = this.WorldToLocal(GetWorldPosition(output));
            Vector2 endPos = this.WorldToLocal(GetWorldPosition(input));

            DrawBezier(painter, startPos, endPos);
        }

        private void DrawBezier(Painter2D painter, Vector2 start, Vector2 end)
        {
            painter.BeginPath();
            painter.MoveTo(start);
            
            float tangentLength = Mathf.Abs(start.x - end.x) * 0.5f;
            if (tangentLength < 50) tangentLength = 50;
            
            Vector2 control1 = start + new Vector2(tangentLength, 0);
            Vector2 control2 = end - new Vector2(tangentLength, 0);
            
            painter.BezierCurveTo(control1, control2, end);
            painter.Stroke();
        }

        private Vector2 GetWorldPosition(VisualElement element)
        {
            if (element == null) return Vector2.zero;
            return element.parent.LocalToWorld(element.layout.center);
        }
    }

