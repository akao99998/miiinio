using UnityEngine;

namespace Swrve.Messaging
{
	public class SwrveMessageRenderer
	{
		protected static readonly Color ButtonPressedColor = new Color(0.5f, 0.5f, 0.5f);

		protected static Texture2D blankTexture;

		protected static Rect WholeScreen = default(Rect);

		public static ISwrveMessageAnimator Animator;

		protected static Texture2D GetBlankTexture()
		{
			if (blankTexture == null)
			{
				blankTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
				blankTexture.SetPixel(0, 0, Color.white);
				blankTexture.SetPixel(1, 0, Color.white);
				blankTexture.SetPixel(0, 1, Color.white);
				blankTexture.SetPixel(1, 1, Color.white);
				blankTexture.Apply(false, true);
			}
			return blankTexture;
		}

		public static void InitMessage(SwrveMessageFormat format)
		{
			if (Animator != null)
			{
				Animator.InitMessage(format);
			}
			else
			{
				format.Init(new Point(0, 0), new Point(0, 0));
			}
		}

		public static void AnimateMessage(SwrveMessageFormat format)
		{
			if (Animator != null)
			{
				Animator.AnimateMessage(format);
			}
		}

		public static void DrawMessage(SwrveMessageFormat format, int centerx, int centery)
		{
			if (Animator != null)
			{
				AnimateMessage(format);
			}
			if (format.BackgroundColor.HasValue && GetBlankTexture() != null)
			{
				Color value = format.BackgroundColor.Value;
				value.a *= format.Message.BackgroundAlpha;
				GUI.color = value;
				WholeScreen.width = Screen.width;
				WholeScreen.height = Screen.height;
				GUI.DrawTexture(WholeScreen, blankTexture, ScaleMode.StretchToFill, true, 10f);
				GUI.color = Color.white;
			}
			if (format.Rotate)
			{
				Vector2 pivotPoint = new Vector2(Screen.width / 2, Screen.height / 2);
				GUIUtility.RotateAroundPivot(90f, pivotPoint);
			}
			float num = format.Scale * format.Message.AnimationScale;
			GUI.color = Color.white;
			foreach (SwrveImage image in format.Images)
			{
				if (image.Texture != null)
				{
					float num2 = num * image.AnimationScale;
					Point centeredPosition = image.GetCenteredPosition(image.Texture.width, image.Texture.height, num2, num);
					centeredPosition.X += centerx;
					centeredPosition.Y += centery;
					image.Rect.x = centeredPosition.X;
					image.Rect.y = centeredPosition.Y;
					image.Rect.width = (float)image.Texture.width * num2;
					image.Rect.height = (float)image.Texture.height * num2;
					GUI.DrawTexture(image.Rect, image.Texture, ScaleMode.StretchToFill, true, 10f);
				}
				else
				{
					GUI.Box(image.Rect, image.File);
				}
			}
			foreach (SwrveButton button in format.Buttons)
			{
				if (button.Texture != null)
				{
					float num3 = num * button.AnimationScale;
					Point centeredPosition2 = button.GetCenteredPosition(button.Texture.width, button.Texture.height, num3, num);
					centeredPosition2.X += centerx;
					centeredPosition2.Y += centery;
					button.Rect.x = centeredPosition2.X;
					button.Rect.y = centeredPosition2.Y;
					button.Rect.width = (float)button.Texture.width * num3;
					button.Rect.height = (float)button.Texture.height * num3;
					if (Animator != null)
					{
						Animator.AnimateButtonPressed(button);
					}
					else
					{
						GUI.color = ((!button.Pressed) ? Color.white : ButtonPressedColor);
					}
					GUI.DrawTexture(button.Rect, button.Texture, ScaleMode.StretchToFill, true, 10f);
				}
				else
				{
					GUI.Box(button.Rect, button.Image);
				}
				GUI.color = Color.white;
			}
			if ((Animator == null && format.Closing) || (Animator != null && Animator.IsMessageDismissed(format)))
			{
				format.Dismissed = true;
				format.UnloadAssets();
			}
		}
	}
}
