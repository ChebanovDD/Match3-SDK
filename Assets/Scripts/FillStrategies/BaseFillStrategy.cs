using System.Collections.Generic;
using Common.Interfaces;
using FillStrategies.Jobs;
using Match3.App.Interfaces;
using Match3.App.Models;
using Match3.Core.Enums;
using Match3.Core.Models;
using Match3.Core.Structs;
using UnityEngine;

namespace FillStrategies
{
    public abstract class BaseFillStrategy : IBoardFillStrategy<IUnityItem>
    {
        private readonly IItemsPool<IUnityItem> _itemsPool;
        private readonly IUnityGameBoardRenderer _gameBoardRenderer;

        protected BaseFillStrategy(IUnityGameBoardRenderer gameBoardRenderer, IItemsPool<IUnityItem> itemsPool)
        {
            _itemsPool = itemsPool;
            _gameBoardRenderer = gameBoardRenderer;
        }

        public abstract string Name { get; }

        public virtual IEnumerable<IJob> GetFillJobs(IGameBoard<IUnityItem> gameBoard)
        {
            var itemsToShow = new List<IUnityItem>();

            for (var rowIndex = 0; rowIndex < gameBoard.RowCount; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < gameBoard.ColumnCount; columnIndex++)
                {
                    var gridSlot = gameBoard[rowIndex, columnIndex];
                    if (CanSetItem(gridSlot) == false)
                    {
                        continue;
                    }

                    var item = _itemsPool.GetItem();
                    item.SetWorldPosition(GetWorldPosition(gridSlot.GridPosition));

                    gridSlot.SetItem(item);
                    itemsToShow.Add(item);
                }
            }

            return new[] { new ItemsShowJob(itemsToShow) };
        }

        public abstract IEnumerable<IJob> GetSolveJobs(IGameBoard<IUnityItem> gameBoard,
            IEnumerable<ItemSequence<IUnityItem>> sequences);

        protected bool CanSetItem(GridSlot<IUnityItem> gridSlot)
        {
            return _gameBoardRenderer.CanSetItem(gridSlot.GridPosition) && gridSlot.State == GridSlotState.Empty;
        }

        protected bool IsMovableSlot(GridSlot<IUnityItem> gridSlot)
        {
            if (_gameBoardRenderer.IsLockedSlot(gridSlot.GridPosition))
            {
                return false;
            }

            return gridSlot.State == GridSlotState.Occupied;
        }

        protected bool IsAvailableSlot(GridSlot<IUnityItem> gridSlot)
        {
            if (_gameBoardRenderer.IsLockedSlot(gridSlot.GridPosition))
            {
                return false;
            }

            return gridSlot.State != GridSlotState.NotAvailable;
        }

        protected Vector3 GetWorldPosition(GridPosition gridPosition)
        {
            return _gameBoardRenderer.GetWorldPosition(gridPosition);
        }
        
        protected bool CanMoveInDirection(IGameBoard<IUnityItem> gameBoard, GridSlot<IUnityItem> gridSlot,
            GridPosition direction, out GridPosition gridPosition)
        {
            var bottomGridSlot = GetSideGridSlot(gameBoard, gridSlot, direction);
            if (bottomGridSlot == null || CanSetItem(bottomGridSlot) == false)
            {
                gridPosition = GridPosition.Zero;
                return false;
            }

            gridPosition = bottomGridSlot.GridPosition;
            return true;
        }

        protected GridSlot<IUnityItem> GetSideGridSlot(IGameBoard<IUnityItem> gameBoard, GridSlot<IUnityItem> gridSlot,
            GridPosition direction)
        {
            var sideGridSlotPosition = gridSlot.GridPosition + direction;

            return gameBoard.IsPositionOnGrid(sideGridSlotPosition)
                ? gameBoard[sideGridSlotPosition]
                : null;
        }
    }
}