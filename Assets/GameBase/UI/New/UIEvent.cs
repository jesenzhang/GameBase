
using UnityEngine;


namespace GameBase
{
    public class UIEvent : MonoBehaviour
    {
        public delegate void VoidDelegate(GameObject go, int id);
        public delegate void BoolDelegate(GameObject go, bool state, int id);
        public delegate void FloatDelegate(GameObject go, float delta, int id);
        public delegate void VectorDelegate(GameObject go, Vector2 delta, int id);
        public delegate void ObjectDelegate(GameObject go, GameObject obj, int id);
        public delegate void KeyCodeDelegate(GameObject go, KeyCode key, int id);

        public int id;

        public VoidDelegate onSubmit;
        public VoidDelegate onClick;
        public VoidDelegate onDoubleClick;
        public BoolDelegate onHover;
        public BoolDelegate onPress;
        public BoolDelegate onSelect;
        public FloatDelegate onScroll;
        public VoidDelegate onDragStart;
        public VectorDelegate onDrag;
        public VoidDelegate onDragOver;
        public VoidDelegate onDragOut;
        public VoidDelegate onDragEnd;
        public ObjectDelegate onDrop;
        public KeyCodeDelegate onKey;
        public BoolDelegate onTooltip;

        public Collider col = null;
        public Collider2D col2D = null;

        bool isColliderEnabled
        {
            get
            {
                if (col == null)
                    col = GetComponent<Collider>();
                if (col != null) return col.enabled;

                if (col2D != null)
                    col2D = GetComponent<Collider2D>();
                return (col2D != null && col2D.enabled);
            }
        }

        protected void OnSubmit() { if (isColliderEnabled && onSubmit != null) onSubmit(gameObject, this.id); }
        protected void OnClick()
        {
            if (isColliderEnabled && onClick != null)
            {
                onClick(gameObject, this.id);
            }
        }

        protected void OnDoubleClick() { if (isColliderEnabled && onDoubleClick != null) onDoubleClick(gameObject, this.id); }
        protected void OnHover(bool isOver) { if (isColliderEnabled && onHover != null) onHover(gameObject, isOver, this.id); }
        protected void OnPress(bool isPressed) { if (isColliderEnabled && onPress != null) onPress(gameObject, isPressed, this.id); }
        protected void OnSelect(bool selected) { if (isColliderEnabled && onSelect != null) onSelect(gameObject, selected, this.id); }
        protected void OnScroll(float delta) { if (isColliderEnabled && onScroll != null) onScroll(gameObject, delta, this.id); }
        protected void OnDragStart() { if (onDragStart != null) onDragStart(gameObject, this.id); }
        protected void OnDrag(Vector2 delta) { if (onDrag != null) onDrag(gameObject, delta, this.id); }
        protected void OnDragOver() { if (isColliderEnabled && onDragOver != null) onDragOver(gameObject, this.id); }
        protected void OnDragOut() { if (isColliderEnabled && onDragOut != null) onDragOut(gameObject, this.id); }
        protected void OnDragEnd() { if (onDragEnd != null) onDragEnd(gameObject, this.id); }
        protected void OnDrop(GameObject go) { if (isColliderEnabled && onDrop != null) onDrop(gameObject, go, this.id); }
        protected void OnKey(KeyCode key) { if (isColliderEnabled && onKey != null) onKey(gameObject, key, this.id); }
        protected void OnTooltip(bool show) { if (isColliderEnabled && onTooltip != null) onTooltip(gameObject, show, this.id); }
    }
}
