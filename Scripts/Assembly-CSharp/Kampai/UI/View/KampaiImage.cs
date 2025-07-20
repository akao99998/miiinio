using System;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class KampaiImage : Image
	{
		[Flags]
		private enum HashCodeFlags
		{
			None = 1,
			IsMaskable = 2,
			HasDesaturate = 4,
			HasTint = 8,
			HasOverlay = 0x10
		}

		public interface IImageCreationArgs
		{
			void SetValues(KampaiImage kampaiImage);
		}

		public class ImageCreationArgs<T> : IImageCreationArgs where T : ImageCreationArgs<T>
		{
			internal Type type;

			protected string image;

			protected string mask;

			protected Transform parentTransform;

			protected LayoutElement layoutElement;

			protected bool isActive = true;

			protected bool isEnabled = true;

			public T SetImage(string imagePath)
			{
				image = imagePath;
				return this as T;
			}

			public T SetEnabled(bool isActive)
			{
				isEnabled = isActive;
				return this as T;
			}

			public T SetActive(bool isActive)
			{
				this.isActive = isActive;
				return this as T;
			}

			public T SetMask(string maskPath)
			{
				mask = maskPath;
				return this as T;
			}

			public T SetParent(Transform parent)
			{
				parentTransform = parent;
				return this as T;
			}

			public T SetLayoutElement(LayoutElement layoutElement)
			{
				this.layoutElement = layoutElement;
				return this as T;
			}

			public virtual void SetValues(KampaiImage kampaiImage)
			{
				RectTransform rectTransform = kampaiImage.GetComponent<RectTransform>() ?? kampaiImage.gameObject.AddComponent<RectTransform>();
				rectTransform.SetParent(parentTransform, false);
				kampaiImage.gameObject.SetActive(isActive);
				kampaiImage.enabled = isEnabled;
				kampaiImage.type = type;
				kampaiImage.sprite = UIUtils.LoadSpriteFromPath(image);
				kampaiImage.maskSprite = UIUtils.LoadSpriteFromPath(mask);
				if (!(this.layoutElement == null))
				{
					LayoutElement layoutElement = kampaiImage.GetComponent<LayoutElement>() ?? kampaiImage.gameObject.AddComponent<LayoutElement>();
					layoutElement.ignoreLayout = this.layoutElement.ignoreLayout;
					if (!layoutElement.ignoreLayout)
					{
						layoutElement.flexibleHeight = this.layoutElement.flexibleHeight;
						layoutElement.flexibleWidth = this.layoutElement.flexibleWidth;
						layoutElement.minHeight = this.layoutElement.minHeight;
						layoutElement.minWidth = this.layoutElement.minWidth;
						layoutElement.preferredHeight = this.layoutElement.preferredHeight;
						layoutElement.preferredWidth = this.layoutElement.preferredWidth;
					}
				}
			}

			private KampaiImage Build(GameObject imageGameObject)
			{
				if (imageGameObject == null)
				{
					return null;
				}
				KampaiImage kampaiImage = imageGameObject.GetComponent<KampaiImage>() ?? imageGameObject.AddComponent<KampaiImage>();
				if (kampaiImage == null)
				{
					return null;
				}
				SetValues(kampaiImage);
				return kampaiImage;
			}

			public KampaiImage CreateKampaiImage(string name)
			{
				return Build(new GameObject(name));
			}

			public KampaiImage CloneKampaiImage(GameObject imageGameObject)
			{
				return Build(UnityEngine.Object.Instantiate(imageGameObject, Vector3.zero, Quaternion.identity) as GameObject);
			}

			public KampaiImage CloneKampaiImage(GameObject imageGameObject, string name)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(imageGameObject, Vector3.zero, Quaternion.identity) as GameObject;
				if (gameObject == null)
				{
					return null;
				}
				gameObject.name = name;
				return Build(gameObject);
			}
		}

		public class SimpleImageCreationArgs : ImageCreationArgs<SimpleImageCreationArgs>
		{
			protected bool preserveAspect;

			public SimpleImageCreationArgs ShouldPreserveAspect(bool preserveAspect)
			{
				this.preserveAspect = preserveAspect;
				return this;
			}

			public override void SetValues(KampaiImage kampaiImage)
			{
				base.SetValues(kampaiImage);
				kampaiImage.preserveAspect = preserveAspect;
			}
		}

		public class SlicedImageCreationArgs : ImageCreationArgs<SlicedImageCreationArgs>
		{
			protected bool fillCenter;

			public SlicedImageCreationArgs ShouldFillCenter(bool fillCenter)
			{
				this.fillCenter = fillCenter;
				return this;
			}

			public override void SetValues(KampaiImage kampaiImage)
			{
				base.SetValues(kampaiImage);
				kampaiImage.fillCenter = fillCenter;
			}
		}

		public class FilledImageCreationArgs : SimpleImageCreationArgs
		{
			protected bool fillClockwise;

			protected float fillAmount;

			protected FillMethod fillMethod;

			protected FillMethod fillOrigin;

			public FilledImageCreationArgs SetFillClockwise(bool fillClockwise)
			{
				this.fillClockwise = fillClockwise;
				return this;
			}

			public FilledImageCreationArgs SetFillOrigin(FillMethod fillOrigin)
			{
				this.fillOrigin = fillOrigin;
				return this;
			}

			public FilledImageCreationArgs SetFillMethod(FillMethod fillMethod)
			{
				this.fillMethod = fillMethod;
				return this;
			}

			public FilledImageCreationArgs SetFillAmount(float fillAmount)
			{
				this.fillAmount = fillAmount;
				return this;
			}

			public override void SetValues(KampaiImage kampaiImage)
			{
				base.SetValues(kampaiImage);
				kampaiImage.fillMethod = fillMethod;
				kampaiImage.fillOrigin = (int)fillOrigin;
				kampaiImage.fillAmount = fillAmount;
				kampaiImage.fillClockwise = fillClockwise;
			}
		}

		public class ImageCreationBuilder
		{
			private IImageCreationArgs args;

			private IImageCreationArgs SetType(Type type)
			{
				switch (type)
				{
				case Type.Simple:
					args = new SimpleImageCreationArgs
					{
						type = type
					};
					break;
				case Type.Sliced:
					args = new SlicedImageCreationArgs
					{
						type = type
					};
					break;
				case Type.Filled:
					args = new FilledImageCreationArgs
					{
						type = type
					};
					break;
				}
				return args;
			}

			public SimpleImageCreationArgs CreateSimpleImage()
			{
				return SetType(Type.Simple) as SimpleImageCreationArgs;
			}

			public SlicedImageCreationArgs CreateSlicedImage()
			{
				return SetType(Type.Sliced) as SlicedImageCreationArgs;
			}

			public FilledImageCreationArgs CreateFilledImage()
			{
				return SetType(Type.Filled) as FilledImageCreationArgs;
			}
		}

		private IKampaiLogger logger = LogManager.GetClassLogger("KampaiImage") as IKampaiLogger;

		[SerializeField]
		private Sprite m_maskSprite;

		private static readonly Vector2[] s_VertScratch = new Vector2[4];

		private static readonly Vector2[] s_Uv = new Vector2[4];

		private static readonly Vector2[] s_Xy = new Vector2[4];

		private static readonly Vector2[] s_UVScratch = new Vector2[4];

		private static readonly Vector2[] s_MaskUVScratch = new Vector2[4];

		private static readonly Vector2[] s_MaskUv = new Vector2[4];

		private static readonly MaterialCache m_cache = new MaterialCache();

		private int m_hashCode;

		private HashCodeFlags m_hashCodeFlags = HashCodeFlags.None;

		private Material myMaterial;

		private float m_Desat;

		private Color m_Tint = Color.white;

		private Color m_Overlay;

		public override Material materialForRendering
		{
			get
			{
				if (myMaterial == null && mainTexture != null)
				{
					m_hashCodeFlags |= (HashCodeFlags)((!base.maskable) ? 1 : 2);
					m_hashCode = string.Format("{0}{1}{2}{3}{4}{5}", mainTexture.name, (!(m_maskSprite == null)) ? m_maskSprite.name : string.Empty, base.maskable, m_Desat, m_Overlay, m_hashCodeFlags).GetHashCode() + material.GetHashCode();
					myMaterial = m_cache.GetMaterial(m_hashCode, material);
				}
				return GetModifiedMaterial(myMaterial ?? material);
			}
		}

		public float Desaturate
		{
			get
			{
				return m_Desat;
			}
			set
			{
				m_Desat = value;
				m_hashCodeFlags |= HashCodeFlags.HasDesaturate;
				myMaterial = null;
				SetMaterialDirty();
			}
		}

		public Color Tint
		{
			get
			{
				return m_Tint;
			}
			set
			{
				m_Tint = value;
				m_hashCodeFlags |= HashCodeFlags.HasTint;
				myMaterial = null;
				SetMaterialDirty();
			}
		}

		public Color Overlay
		{
			get
			{
				return m_Overlay;
			}
			set
			{
				m_Overlay = value;
				m_hashCodeFlags |= HashCodeFlags.HasOverlay;
				myMaterial = null;
				SetMaterialDirty();
			}
		}

		public Sprite maskSprite
		{
			get
			{
				return m_maskSprite;
			}
			set
			{
				if (!(m_maskSprite == value))
				{
					m_maskSprite = value;
					myMaterial = null;
					SetAllDirty();
				}
			}
		}

		protected override void OnDestroy()
		{
			m_cache.RemoveReference(m_hashCode);
			base.OnDestroy();
		}

		protected override void OnPopulateMesh(VertexHelper vbo)
		{
			if (base.overrideSprite == null)
			{
				base.OnPopulateMesh(vbo);
				return;
			}
			vbo.Clear();
			switch (base.type)
			{
			case Type.Simple:
				GenerateSimpleSprite(vbo, base.preserveAspect);
				break;
			case Type.Sliced:
				GenerateSlicedSprite(vbo);
				break;
			case Type.Tiled:
				logger.Error("Tiled image type is not supported on KampaiImage");
				break;
			case Type.Filled:
				GenerateFilledSprite(vbo, base.preserveAspect);
				break;
			}
		}

		protected override void UpdateMaterial()
		{
			base.UpdateMaterial();
			Material material = materialForRendering;
			if (IsActive() && !(material == null))
			{
				material.SetTexture(GameConstants.ShaderProperties.UI.AlphaTex, (!(m_maskSprite != null)) ? Texture2D.whiteTexture : m_maskSprite.texture);
				if (material.HasProperty(GameConstants.ShaderProperties.UI.Overlay))
				{
					material.SetColor(GameConstants.ShaderProperties.UI.Overlay, m_Overlay);
				}
				else if (logger != null)
				{
					logger.Error("Unable to set overlay: property on material for texture {0} does not exist.", GetSafeSpriteTextureName(base.sprite));
				}
				if (material.HasProperty(GameConstants.ShaderProperties.UI.Desaturation))
				{
					material.SetFloat(GameConstants.ShaderProperties.UI.Desaturation, m_Desat);
				}
				else if (logger != null)
				{
					logger.Error("Unable to set desaturation: property on material for texture {0} does not exist.", GetSafeSpriteTextureName(base.sprite));
				}
				material.color = m_Tint;
			}
		}

		private static string GetSafeSpriteTextureName(Sprite sprite)
		{
			if (sprite == null)
			{
				return "(sprite is null)";
			}
			if (sprite.texture == null)
			{
				return "(sprite.texture is null)";
			}
			if (sprite.texture.name == null)
			{
				return "(sprite.texture.name is null)";
			}
			return sprite.texture.name;
		}

		private void GenerateSimpleSprite(VertexHelper vbo, bool preserveAspect)
		{
			Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);
			Vector4 vector = ((!(base.overrideSprite != null)) ? Vector4.zero : DataUtility.GetOuterUV(base.overrideSprite));
			Vector4 vector2 = ((m_maskSprite == null) ? Vector4.zero : ((!m_maskSprite.packed) ? vector : DataUtility.GetOuterUV(m_maskSprite)));
			vbo.Clear();
			vbo.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.y), base.color, new Vector2(vector.x, vector.y), new Vector2(vector2.x, vector2.y));
			vbo.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.w), base.color, new Vector2(vector.x, vector.w), new Vector2(vector2.x, vector2.w));
			vbo.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.w), base.color, new Vector2(vector.z, vector.w), new Vector2(vector2.z, vector2.w));
			vbo.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.y), base.color, new Vector2(vector.z, vector.y), new Vector2(vector2.z, vector2.y));
			vbo.AddTriangle(0, 1, 2);
			vbo.AddTriangle(2, 3, 0);
		}

		private void GenerateSlicedSprite(VertexHelper vbo)
		{
			if (!base.hasBorder)
			{
				GenerateSimpleSprite(vbo, false);
				return;
			}
			Vector4 zero = Vector4.zero;
			Vector4 vector;
			Vector4 vector2;
			Vector4 vector3;
			Vector4 vector4;
			if (base.overrideSprite != null)
			{
				vector = DataUtility.GetOuterUV(base.overrideSprite);
				vector2 = DataUtility.GetInnerUV(base.overrideSprite);
				vector3 = DataUtility.GetPadding(base.overrideSprite);
				vector4 = base.overrideSprite.border;
			}
			else
			{
				vector = zero;
				vector2 = zero;
				vector3 = zero;
				vector4 = zero;
			}
			Vector4 vector5;
			Vector4 vector6;
			if (m_maskSprite != null)
			{
				if (m_maskSprite.packed)
				{
					vector5 = DataUtility.GetOuterUV(m_maskSprite);
					vector6 = DataUtility.GetInnerUV(m_maskSprite);
				}
				else
				{
					vector5 = vector;
					vector6 = vector2;
				}
			}
			else
			{
				vector5 = zero;
				vector6 = zero;
			}
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			vector4 = GetAdjustedBorders(vector4 / base.pixelsPerUnit, pixelAdjustedRect);
			vector3 /= base.pixelsPerUnit;
			s_VertScratch[0] = new Vector2(vector3.x, vector3.y);
			s_VertScratch[3] = new Vector2(pixelAdjustedRect.width - vector3.z, pixelAdjustedRect.height - vector3.w);
			s_VertScratch[1].x = vector4.x;
			s_VertScratch[1].y = vector4.y;
			s_VertScratch[2].x = pixelAdjustedRect.width - vector4.z;
			s_VertScratch[2].y = pixelAdjustedRect.height - vector4.w;
			for (int i = 0; i < 4; i++)
			{
				s_VertScratch[i].x += pixelAdjustedRect.x;
				s_VertScratch[i].y += pixelAdjustedRect.y;
			}
			s_UVScratch[0] = new Vector2(vector.x, vector.y);
			s_UVScratch[1] = new Vector2(vector2.x, vector2.y);
			s_UVScratch[2] = new Vector2(vector2.z, vector2.w);
			s_UVScratch[3] = new Vector2(vector.z, vector.w);
			s_MaskUVScratch[0] = new Vector2(vector5.x, vector5.y);
			s_MaskUVScratch[1] = new Vector2(vector6.x, vector6.y);
			s_MaskUVScratch[2] = new Vector2(vector6.z, vector6.w);
			s_MaskUVScratch[3] = new Vector2(vector5.z, vector5.w);
			vbo.Clear();
			for (int j = 0; j < 3; j++)
			{
				int num = j + 1;
				for (int k = 0; k < 3; k++)
				{
					if (base.fillCenter || j != 1 || k != 1)
					{
						int num2 = k + 1;
						AddQuad(vbo, new Vector2(s_VertScratch[j].x, s_VertScratch[k].y), new Vector2(s_VertScratch[num].x, s_VertScratch[num2].y), base.color, new Vector2(s_UVScratch[j].x, s_UVScratch[k].y), new Vector2(s_UVScratch[num].x, s_UVScratch[num2].y), new Vector2(s_MaskUVScratch[j].x, s_MaskUVScratch[k].y), new Vector2(s_MaskUVScratch[num].x, s_MaskUVScratch[num2].y));
					}
				}
			}
		}

		private void GenerateFilledSprite(VertexHelper vbo, bool preserveAspect)
		{
			if (base.fillAmount < 0.001f)
			{
				return;
			}
			Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);
			Vector4 vector = ((base.overrideSprite != null) ? DataUtility.GetOuterUV(base.overrideSprite) : Vector4.zero);
			Vector4 vector2 = ((!m_maskSprite.packed) ? vector : ((!(m_maskSprite == null)) ? DataUtility.GetOuterUV(m_maskSprite) : Vector4.zero));
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.color = base.color;
			float x = vector.x;
			float y = vector.y;
			float z = vector.z;
			float w = vector.w;
			float x2 = vector2.x;
			float y2 = vector2.y;
			float z2 = vector2.z;
			float w2 = vector2.w;
			ProcessHorizontalOrVerticleFill(x, y, z, w, x2, y2, z2, w2, drawingDimensions);
			if (base.fillAmount < 1f)
			{
				switch (base.fillMethod)
				{
				case FillMethod.Radial90:
					ProcessRadialFill_90(vbo, simpleVert);
					return;
				case FillMethod.Radial180:
					ProcessRadialFill_180(vbo, simpleVert, x, y, z, w, x2, y2, z2, w2, drawingDimensions);
					return;
				case FillMethod.Radial360:
					ProcessRadialFill_360(vbo, simpleVert, x, y, z, w, x2, y2, z2, w2, drawingDimensions);
					return;
				}
			}
			SetupVBO(vbo, simpleVert);
		}

		private void ProcessHorizontalOrVerticleFill(float num, float num2, float num3, float num4, float maskNum, float maskNum2, float maskNum3, float maskNum4, Vector4 drawingDimensions)
		{
			if (base.fillMethod == FillMethod.Horizontal || base.fillMethod == FillMethod.Vertical)
			{
				switch (base.fillMethod)
				{
				case FillMethod.Horizontal:
				{
					float num7 = (num3 - num) * base.fillAmount;
					float num8 = (maskNum3 - maskNum) * base.fillAmount;
					if (base.fillOrigin == 1)
					{
						drawingDimensions.x = drawingDimensions.z - (drawingDimensions.z - drawingDimensions.x) * base.fillAmount;
						num = num3 - num7;
						maskNum = maskNum3 - num8;
					}
					else
					{
						drawingDimensions.z = drawingDimensions.x + (drawingDimensions.z - drawingDimensions.x) * base.fillAmount;
						num3 = num + num7;
						maskNum3 = maskNum + num8;
					}
					break;
				}
				case FillMethod.Vertical:
				{
					float num5 = (num4 - num2) * base.fillAmount;
					float num6 = (maskNum4 - maskNum2) * base.fillAmount;
					if (base.fillOrigin == 1)
					{
						drawingDimensions.y = drawingDimensions.w - (drawingDimensions.w - drawingDimensions.y) * base.fillAmount;
						num2 = num4 - num5;
						maskNum2 = maskNum4 - num6;
					}
					else
					{
						drawingDimensions.w = drawingDimensions.y + (drawingDimensions.w - drawingDimensions.y) * base.fillAmount;
						num4 = num2 + num5;
						maskNum4 = maskNum2 + num6;
					}
					break;
				}
				}
			}
			s_Xy[0] = new Vector2(drawingDimensions.x, drawingDimensions.y);
			s_Xy[1] = new Vector2(drawingDimensions.x, drawingDimensions.w);
			s_Xy[2] = new Vector2(drawingDimensions.z, drawingDimensions.w);
			s_Xy[3] = new Vector2(drawingDimensions.z, drawingDimensions.y);
			s_Uv[0] = new Vector2(num, num2);
			s_Uv[1] = new Vector2(num, num4);
			s_Uv[2] = new Vector2(num3, num4);
			s_Uv[3] = new Vector2(num3, num2);
			s_MaskUv[0] = new Vector2(maskNum, maskNum2);
			s_MaskUv[1] = new Vector2(maskNum, maskNum4);
			s_MaskUv[2] = new Vector2(maskNum3, maskNum4);
			s_MaskUv[3] = new Vector2(maskNum3, maskNum2);
		}

		private void ProcessRadialFill_90(VertexHelper vbo, UIVertex simpleVert)
		{
			if (RadialCut(s_Xy, s_Uv, s_MaskUv, base.fillAmount, base.fillClockwise, base.fillOrigin))
			{
				SetupVBO(vbo, simpleVert);
			}
		}

		private void ProcessRadialFill_180(VertexHelper vbo, UIVertex simpleVert, float num, float num2, float num3, float num4, float maskNum, float maskNum2, float maskNum3, float maskNum4, Vector4 drawingDimensions)
		{
			for (int i = 0; i < 2; i++)
			{
				int num5 = ((base.fillOrigin > 1) ? 1 : 0);
				float custom;
				float custom2;
				float custom3;
				float custom4;
				if (base.fillOrigin == 0 || base.fillOrigin == 2)
				{
					custom = 0f;
					custom2 = 1f;
					if (i == num5)
					{
						custom3 = 0f;
						custom4 = 0.5f;
					}
					else
					{
						custom3 = 0.5f;
						custom4 = 1f;
					}
				}
				else
				{
					custom3 = 0f;
					custom4 = 1f;
					if (i == num5)
					{
						custom = 0.5f;
						custom2 = 1f;
					}
					else
					{
						custom = 0f;
						custom2 = 0.5f;
					}
				}
				SetupKampaiImageProperties(num, num2, num3, num4, maskNum, maskNum2, maskNum3, maskNum4, drawingDimensions, custom3, custom4, custom, custom2);
				float value = (base.fillClockwise ? (base.fillAmount * 2f - (float)i) : (base.fillAmount * 2f - (float)(1 - i)));
				if (RadialCut(s_Xy, s_Uv, s_MaskUv, Mathf.Clamp01(value), base.fillClockwise, (i + base.fillOrigin + 3) % 4))
				{
					SetupVBO(vbo, simpleVert);
				}
			}
		}

		private void ProcessRadialFill_360(VertexHelper vbo, UIVertex simpleVert, float num, float num2, float num3, float num4, float maskNum, float maskNum2, float maskNum3, float maskNum4, Vector4 drawingDimensions)
		{
			for (int i = 0; i < 4; i++)
			{
				float custom;
				float custom2;
				if (i < 2)
				{
					custom = 0f;
					custom2 = 0.5f;
				}
				else
				{
					custom = 0.5f;
					custom2 = 1f;
				}
				float custom3;
				float custom4;
				if (i == 0 || i == 3)
				{
					custom3 = 0f;
					custom4 = 0.5f;
				}
				else
				{
					custom3 = 0.5f;
					custom4 = 1f;
				}
				SetupKampaiImageProperties(num, num2, num3, num4, maskNum, maskNum2, maskNum3, maskNum4, drawingDimensions, custom, custom2, custom3, custom4);
				float value = (base.fillClockwise ? (base.fillAmount * 4f - (float)((i + base.fillOrigin) % 4)) : (base.fillAmount * 4f - (float)(3 - (i + base.fillOrigin) % 4)));
				if (RadialCut(s_Xy, s_Uv, s_MaskUv, Mathf.Clamp01(value), base.fillClockwise, (i + 2) % 4))
				{
					SetupVBO(vbo, simpleVert);
				}
			}
		}

		private void SetupKampaiImageProperties(float num, float num2, float num3, float num4, float maskNum, float maskNum2, float maskNum3, float maskNum4, Vector4 drawingDimensions, float custom1, float custom2, float custom3, float custom4)
		{
			s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, custom1);
			s_Xy[1].x = s_Xy[0].x;
			s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, custom2);
			s_Xy[3].x = s_Xy[2].x;
			s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, custom3);
			s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, custom4);
			s_Xy[2].y = s_Xy[1].y;
			s_Xy[3].y = s_Xy[0].y;
			s_Uv[0].x = Mathf.Lerp(num, num3, custom1);
			s_Uv[1].x = s_Uv[0].x;
			s_Uv[2].x = Mathf.Lerp(num, num3, custom2);
			s_Uv[3].x = s_Uv[2].x;
			s_Uv[0].y = Mathf.Lerp(num2, num4, custom3);
			s_Uv[1].y = Mathf.Lerp(num2, num4, custom4);
			s_Uv[2].y = s_Uv[1].y;
			s_Uv[3].y = s_Uv[0].y;
			s_MaskUv[0].x = Mathf.Lerp(maskNum, maskNum3, custom1);
			s_MaskUv[1].x = s_MaskUv[0].x;
			s_MaskUv[2].x = Mathf.Lerp(maskNum, maskNum3, custom2);
			s_MaskUv[3].x = s_MaskUv[2].x;
			s_MaskUv[0].y = Mathf.Lerp(maskNum2, maskNum4, custom3);
			s_MaskUv[1].y = Mathf.Lerp(maskNum2, maskNum4, custom4);
			s_MaskUv[2].y = s_MaskUv[1].y;
			s_MaskUv[3].y = s_MaskUv[0].y;
		}

		private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
		{
			for (int i = 0; i <= 1; i++)
			{
				float num = border[i] + border[i + 2];
				if (rect.size[i] < num && num != 0f)
				{
					float num2 = rect.size[i] / num;
					int index;
					int index2 = (index = i);
					float num3 = border[index];
					border[index2] = num3 * num2;
					int index3 = (index = i + 2);
					num3 = border[index];
					border[index3] = num3 * num2;
				}
			}
			return border;
		}

		private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
		{
			Vector4 vector = ((base.overrideSprite == null) ? Vector4.zero : DataUtility.GetPadding(base.overrideSprite));
			Vector2 vector2 = ((base.overrideSprite == null) ? Vector2.zero : new Vector2(base.overrideSprite.rect.width, base.overrideSprite.rect.height));
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			int num = Mathf.RoundToInt(vector2.x);
			int num2 = Mathf.RoundToInt(vector2.y);
			Vector4 vector3 = new Vector4(vector.x / (float)num, vector.y / (float)num2, ((float)num - vector.z) / (float)num, ((float)num2 - vector.w) / (float)num2);
			if (shouldPreserveAspect && vector2.sqrMagnitude > 0f)
			{
				float num3 = vector2.x / vector2.y;
				float num4 = pixelAdjustedRect.width / pixelAdjustedRect.height;
				if (num3 > num4)
				{
					float height = pixelAdjustedRect.height;
					pixelAdjustedRect.height = pixelAdjustedRect.width * (1f / num3);
					pixelAdjustedRect.y += (height - pixelAdjustedRect.height) * base.rectTransform.pivot.y;
				}
				else
				{
					float width = pixelAdjustedRect.width;
					pixelAdjustedRect.width = pixelAdjustedRect.height * num3;
					pixelAdjustedRect.x += (width - pixelAdjustedRect.width) * base.rectTransform.pivot.x;
				}
			}
			return new Vector4(pixelAdjustedRect.x + pixelAdjustedRect.width * vector3.x, pixelAdjustedRect.y + pixelAdjustedRect.height * vector3.y, pixelAdjustedRect.x + pixelAdjustedRect.width * vector3.z, pixelAdjustedRect.y + pixelAdjustedRect.height * vector3.w);
		}

		private static void RadialCut(Vector2[] xy, float cos, float sin, bool invert, int corner)
		{
			int num = (corner + 1) % 4;
			int num2 = (corner + 2) % 4;
			int num3 = (corner + 3) % 4;
			if ((corner & 1) == 1)
			{
				if (sin > cos)
				{
					cos /= sin;
					sin = 1f;
					if (invert)
					{
						xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
						xy[num2].x = xy[num].x;
					}
				}
				else if (cos > sin)
				{
					sin /= cos;
					cos = 1f;
					if (!invert)
					{
						xy[num2].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
						xy[num3].y = xy[num2].y;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}
				if (!invert)
				{
					xy[num3].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
				}
				else
				{
					xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
				}
				return;
			}
			if (cos > sin)
			{
				sin /= cos;
				cos = 1f;
				if (!invert)
				{
					xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
					xy[num2].y = xy[num].y;
				}
			}
			else if (sin > cos)
			{
				cos /= sin;
				sin = 1f;
				if (invert)
				{
					xy[num2].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
					xy[num3].x = xy[num2].x;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}
			if (invert)
			{
				xy[num3].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
			}
			else
			{
				xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
			}
		}

		private static bool RadialCut(Vector2[] xy, Vector2[] uv, Vector2[] maskUV, float fill, bool invert, int corner)
		{
			if (fill < 0.001f)
			{
				return false;
			}
			if ((corner & 1) == 1)
			{
				invert = !invert;
			}
			if (!invert && fill > 0.999f)
			{
				return true;
			}
			float num = Mathf.Clamp01(fill);
			if (invert)
			{
				num = 1f - num;
			}
			num *= (float)Math.PI / 2f;
			float cos = Mathf.Cos(num);
			float sin = Mathf.Sin(num);
			RadialCut(xy, cos, sin, invert, corner);
			RadialCut(uv, cos, sin, invert, corner);
			RadialCut(maskUV, cos, sin, invert, corner);
			return true;
		}

		public void SetStencilMaterial()
		{
			material = KampaiResources.Load<Material>("StencilAlphaMaskMat");
		}

		private static void SetupVBO(VertexHelper vertexHelper, UIVertex simpleVert)
		{
			int currentVertCount = vertexHelper.currentVertCount;
			for (int i = 0; i < 4; i++)
			{
				vertexHelper.AddVert(s_Xy[i], simpleVert.color, s_Uv[1], s_MaskUv[i]);
			}
			vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
			vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
		}

		private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax, Vector2 uvMaskMin, Vector2 uvMaskMax)
		{
			int currentVertCount = vertexHelper.currentVertCount;
			vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y), new Vector2(uvMaskMin.x, uvMaskMin.y));
			vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y), new Vector2(uvMaskMin.x, uvMaskMax.y));
			vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y), new Vector2(uvMaskMax.x, uvMaskMax.y));
			vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y), new Vector2(uvMaskMax.x, uvMaskMin.y));
			vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
			vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
		}
	}
}
