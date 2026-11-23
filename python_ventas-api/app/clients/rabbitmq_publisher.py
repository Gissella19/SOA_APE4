"""Publicador independiente de eventos de venta confirmada hacia RabbitMQ."""
import json
import logging
import pika
from app.config import (
    RABBITMQ_HOST,
    RABBITMQ_PORT,
    RABBITMQ_USER,
    RABBITMQ_PASSWORD,
    RABBITMQ_QUEUE,
)

logger = logging.getLogger(__name__)


def publicar_evento_venta(evento: dict) -> bool:
    """Publica un mensaje persistente en la cola durable de RabbitMQ."""
    conexion = None
    try:
        credenciales = pika.PlainCredentials(RABBITMQ_USER, RABBITMQ_PASSWORD)
        parametros = pika.ConnectionParameters(
            host=RABBITMQ_HOST,
            port=RABBITMQ_PORT,
            credentials=credenciales,
            connection_attempts=1,
            retry_delay=0,
            socket_timeout=3,
        )
        conexion = pika.BlockingConnection(parametros)
        canal = conexion.channel()
        canal.queue_declare(queue=RABBITMQ_QUEUE, durable=True)

        cuerpo = json.dumps(evento, ensure_ascii=False).encode("utf-8")
        propiedades = pika.BasicProperties(
            delivery_mode=2,
            content_type="application/json",
            message_id=str(evento.get("eventoId", "")),
        )
        canal.basic_publish(
            exchange="",
            routing_key=RABBITMQ_QUEUE,
            body=cuerpo,
            properties=propiedades,
        )
        return True
    except Exception as exc:
        logger.warning(
            "No fue posible publicar el evento %s en RabbitMQ (%s:%s): %s",
            evento.get("eventoId"),
            RABBITMQ_HOST,
            RABBITMQ_PORT,
            type(exc).__name__,
        )
        return False
    finally:
        if conexion is not None and conexion.is_open:
            try:
                conexion.close()
            except Exception:
                pass
