using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay {
    public class PlayerDeath : Simulation.Event<PlayerDeath> {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute() {
            var player = model.player;
            if (player.animator.GetBool("dead")) return;
            //if (player.health.IsAlive) {
            player.health.Die();
            model.virtualCamera.Follow = null;
            model.virtualCamera.LookAt = null;
            player.controlEnabled = false;

            if (player.audioSource && player.ouchAudio)
                player.audioSource.PlayOneShot(player.ouchAudio);

            player.animator.SetTrigger("hurt");
            player.animator.SetBool("dead", true);

            //var impulse = player.GetComponent<Unity.Cinemachine.CinemachineImpulseSource>();
            //if (impulse != null) impulse.GenerateImpulse();

            GameLoopManager loopManager = Object.FindFirstObjectByType<GameLoopManager>();
            if (loopManager != null) {
                bool hasLivesLeft = loopManager.OnPlayerDied();
                if (hasLivesLeft) {
                    Simulation.Schedule<PlayerSpawn>(2);
                } else {
                    loopManager.ShowGameOver();
                }
            } else {
                Simulation.Schedule<PlayerSpawn>(2);
            }
            //}
        }
    }
}