using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Pagination
{
    public partial class PagedRect
    {
        private PointerEventData lastEndDragData;
        private float dragStartTime = 0f;
        private int currentPageBeforeDragStarted = 1;

        public void OnBeginDrag(PointerEventData data)
        {
            if (!UsingScrollRect) return;

            dragStartTime = Time.time;
            currentPageBeforeDragStarted = CurrentPage;
        }

        /// <summary>
        /// Called whenever the user stops dragging
        /// </summary>
        /// <param name="data"></param>
        public void OnEndDrag(PointerEventData data)
        {            
            if (!UsingScrollRect) return;
            if (lastEndDragData == data) return; // ignore repeated OnEndDrag calls

            lastEndDragData = data;            

            if (LoopSeamlessly && !ShowPagePreviews)
            {                    
                // Slightly different logic if we're on the first or last pages
                var pagePosition = GetPagePosition(CurrentPage);
                if (pagePosition == 1 || pagePosition == NumberOfPages)
                {
                    var direction = GetDragDeltaDirection(data);
                    if (direction == DeltaDirection.Next && pagePosition == NumberOfPages)
                    {                        
                        MoveFirstPageToEnd();
                        PreviousPage(); 
                    }
                    else if (direction == DeltaDirection.Previous && pagePosition == 1)
                    {
                        MoveLastPageToStart();                        
                        NextPage();                        
                    }
                }
                else
                {
                    UpdateSeamlessPagePositions();
                }                
            }

            // New as of v1.36
            // Don't attempt process long duration drags as swipes
            // (as this causes us to only move by 1 page when we may have actually dragged the scrollrect quite far, and we basically 'snap back' to the page)
            if (UseSwipeInputForScrollRect && Mathf.Abs(Time.time - dragStartTime) <= 0.25f)
            {
                if (HandleDragDelta(data)) return;
            }

            CurrentPage = currentPageBeforeDragStarted;
            ScrollToClosestPage();
        }

        protected void ScrollToClosestPage()
        {            
            // If we dragged less than the delta threshold, we may still be between pages - the following code will either return us to our previous page,
            // or take us to the next/previous if they are closer
            var pageDistances = GetPageDistancesFromScrollRectCenter();
            
            if (pageDistances.Any())
            {
                var closestPage = pageDistances.OrderBy(p => p.Value).First().Key;

                //Debug.Log(String.Join(", ", pageDistances.OrderBy(p => p.Value).Select(p => String.Format("{0} [{1}]", p.Value, p.Key)).ToArray()));

                SetCurrentPage(closestPage);
            }
        }

        protected DeltaDirection GetDragDeltaDirection(PointerEventData data)
        {            
            bool goToNextPage = false;
            bool goToPreviousPage = false;
            if (ScrollRect.horizontal)
            {
                // Ignore any vertical swipes (where they y delta is greater than the x)
                if (Mathf.Abs(data.delta.y) > Mathf.Abs(data.delta.x)) return DeltaDirection.None;

                if (data.delta.x > SwipeDeltaThreshold)
                {
                    goToPreviousPage = true;
                }
                else if (data.delta.x < -SwipeDeltaThreshold)
                {
                    goToNextPage = true;
                }
                else
                {
                    goToPreviousPage = _ScrollRectPosition.x < 0f;
                    goToNextPage = _ScrollRectPosition.x > 1f;
                }                
            }
            else if (ScrollRect.vertical)
            {
                // Ignore any horizontal swipes (where the x delta is greater than the y)
                if (Mathf.Abs(data.delta.x) > Mathf.Abs(data.delta.y)) return DeltaDirection.None;

                if (data.delta.y > SwipeDeltaThreshold)
                {
                    goToPreviousPage = true;
                }
                else if (data.delta.y < -SwipeDeltaThreshold)
                {
                    goToNextPage = true;
                }
                else
                {
                    goToPreviousPage = _ScrollRectPosition.y < 0f;
                    goToNextPage = _ScrollRectPosition.y > 1f;
                }
            }

            if (goToNextPage) return DeltaDirection.Next;
            if (goToPreviousPage) return DeltaDirection.Previous;

            return DeltaDirection.None;
        }
        
        internal bool HandleDragDelta(PointerEventData data)
        {            
            var deltaDirection = GetDragDeltaDirection(data);

            return HandleDragDelta(deltaDirection);
        }

        protected bool HandleDragDelta(DeltaDirection deltaDirection)
        {            
            if (deltaDirection == DeltaDirection.Next)
            {
                if(CurrentPage != NumberOfPages || LoopEndlessly) NextPage();
                return true;
            }
            else if (deltaDirection == DeltaDirection.Previous)
            {
                if(CurrentPage != 1 || LoopEndlessly) PreviousPage();
                return true;
            }

            return false;
        }

        public void OnDrag(PointerEventData data)
        {
            if (!UsingScrollRect) return;

            if (ShowPagePreviews)
            {
                HandleDrag_PagePreviews();
                return;
            }

            var closestPage = GetClosestPageNumberToScrollRectCenter();
            if (CurrentPage != closestPage)
            {
                CurrentPage = closestPage;
                UpdatePagination();
                UpdateScrollBarPosition();
                UpdateSeamlessPagePositions();
            }
        }
    }
}
