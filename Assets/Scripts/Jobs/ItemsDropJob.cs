﻿using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interfaces;
using Models;
using UnityEngine;

namespace Jobs
{
    public class ItemsDropJob : DropJob
    {
        private const float FadeDuration = 0.15f;
        private const float IntervalDuration = 0.25f;

        private readonly float _delay;
        private readonly List<ItemDropData> _itemsData;

        public ItemsDropJob(List<ItemDropData> items, bool useDelay)
        {
            _itemsData = items;
            _delay = useDelay ? IntervalDuration : 0;
        }

        public override async UniTask ExecuteAsync()
        {
            var itemsSequence = DOTween.Sequence();

            foreach (var itemData in _itemsData)
            {
                SetTransparent(itemData.Item, 0);
                itemData.Item.Transform.localScale = Vector3.one;
                itemData.Item.Show();

                var itemDropSequence = CreateItemMoveSequence(itemData);
                _ = itemsSequence
                    .Join(itemData.Item.SpriteRenderer.DOFade(1, FadeDuration))
                    .Join(itemDropSequence).PrependInterval(itemDropSequence.Duration() * IntervalDuration);
            }

            await itemsSequence
                .SetDelay(_delay, false)
                .SetEase(Ease.Flash);
        }

        private void SetTransparent(IItem item, float value) // TODO: Extension?
        {
            var color = item.SpriteRenderer.color;
            item.SpriteRenderer.color = new Color(color.r, color.g, color.b, value);
        }
    }
}