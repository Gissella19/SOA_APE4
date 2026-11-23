package ec.edu.uta.notificaciones.repository;

import java.util.List;
import java.util.Optional;
import org.springframework.data.jpa.repository.JpaRepository;
import ec.edu.uta.notificaciones.model.Notificacion;

public interface NotificacionRepository extends JpaRepository<Notificacion, Integer> {

    boolean existsByEventoId(String eventoId);

    Optional<Notificacion> findByEventoId(String eventoId);

    List<Notificacion> findAllByOrderByIdDesc();
}
