﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	public class FirstPersonView : BaseView
	{
		public float HeightOffset { get; set; }
		public float CuePullBackOffset { get; set; }

		public override void UpdateCamera( PoolCamera camera )
		{
			var whiteBall = Game.Instance.WhiteBall;
			var cue = Viewer.Cue;

			if ( whiteBall.IsValid && cue.IsValid )
			{
				camera.Pos = cue.Entity.WorldPos + new Vector3( 0f, 0f, 50f + HeightOffset );
				var difference = (whiteBall.Entity.WorldPos - camera.Pos).Normal;
				camera.Rot = Rotation.LookAt( difference, Vector3.Up );
			}
			else
			{
				camera.Pos = camera.Pos.LerpTo( new Vector3( 0f, 0f, 2048f ), Time.Delta );
				camera.Rot = Rotation.Lerp( camera.Rot, Rotation.LookAt( Vector3.Down ), Time.Delta );
			}
		}

		public override void Tick()
		{
			if ( Viewer.IsFollowingBall )
				return;

			var whiteBall = Game.Instance.WhiteBall;
			var input = Viewer.Input;
			var cue = Viewer.Cue;

			if ( !whiteBall.IsValid || !cue.IsValid )
				return;

			if ( !input.Down( InputButton.Attack1 ) )
			{
				var yaw = input.Rot.Yaw();
				cue.Entity.WorldRot = Rotation.From( cue.Entity.WorldRot.Angles().WithYaw( yaw ) );
			}
			else
			{
				CuePullBackOffset = Math.Clamp( CuePullBackOffset + input.MouseDelta.y, -10f, 60f );

				if ( CuePullBackOffset < -5f )
				{
					using ( Prediction.Off() )
					{
						// TODO: This is shit, it will likely be physics based.
						var force = input.MouseDelta.y * 200f;
						Viewer.StrikeWhiteBall( cue, whiteBall, force );
						return;
					}
				}
			}

			cue.Entity.WorldPos = whiteBall.Entity.WorldPos - cue.Entity.WorldRot.Left * (250f + CuePullBackOffset);
		}

		public override void BuildInput( ClientInput input )
		{
			if ( !input.Down( InputButton.Attack1 ) )
			{
				HeightOffset = Math.Clamp( HeightOffset + input.AnalogLook.pitch, 0f, 200f );
			}
		}

		public override void OnWhiteBallStruck( PoolCue cue, PoolBall whiteBall, float force )
		{
			CuePullBackOffset = 0f;
		}
	}
}