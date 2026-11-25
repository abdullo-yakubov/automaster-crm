import { useEffect, useState } from 'react';
import { DragDropContext, Droppable, Draggable } from '@hello-pangea/dnd';
import type { DropResult } from '@hello-pangea/dnd';
import toast from 'react-hot-toast';
import { ordersService } from '../../services/orders.service';
import { LicensePlate } from '../../components/UI/Badge';
import { STATUS_LABELS_RU } from '../../types';
import type { Order } from '../../types';

export function KanbanPage() {
  const [columns, setColumns] = useState<{ status: string; orders: Order[] }[]>([]);

  async function load() {
    const data = await ordersService.kanban();
    setColumns(data);
  }

  useEffect(() => {
    load();
  }, []);

  async function handleDragEnd(result: DropResult) {
    const { source, destination, draggableId } = result;
    if (!destination || source.droppableId === destination.droppableId) return;

    const newStatus = destination.droppableId;
    setColumns((cols) => {
      const next = cols.map((c) => ({ ...c, orders: [...c.orders] }));
      const fromCol = next.find((c) => c.status === source.droppableId)!;
      const toCol = next.find((c) => c.status === newStatus)!;
      const idx = fromCol.orders.findIndex((o) => o.id === draggableId);
      const [moved] = fromCol.orders.splice(idx, 1);
      moved.status = newStatus;
      toCol.orders.splice(destination.index, 0, moved);
      return next;
    });

    try {
      await ordersService.updateStatus(draggableId, newStatus);
    } catch {
      toast.error('Не удалось изменить статус');
      load();
    }
  }

  return (
    <div>
      <h2>Kanban</h2>
      <DragDropContext onDragEnd={handleDragEnd}>
        <div className="kanban-board">
          {columns.map((col) => (
            <Droppable droppableId={col.status} key={col.status}>
              {(provided) => (
                <div className="kanban-col" ref={provided.innerRef} {...provided.droppableProps}>
                  <div className="kanban-col-header">
                    {STATUS_LABELS_RU[col.status] || col.status} ({col.orders.length})
                  </div>
                  {col.orders.map((o, idx) => (
                    <Draggable draggableId={o.id} index={idx} key={o.id}>
                      {(p) => (
                        <div className="kanban-card" ref={p.innerRef} {...p.draggableProps} {...p.dragHandleProps}>
                          <div><strong>{o.orderNumber}</strong></div>
                          <div>{o.customerName}</div>
                          <div>{o.vehicleMakeModel} <LicensePlate value={o.licensePlate} /></div>
                          <div>{o.totalCost.toLocaleString()} TJS</div>
                        </div>
                      )}
                    </Draggable>
                  ))}
                  {provided.placeholder}
                </div>
              )}
            </Droppable>
          ))}
        </div>
      </DragDropContext>
    </div>
  );
}
