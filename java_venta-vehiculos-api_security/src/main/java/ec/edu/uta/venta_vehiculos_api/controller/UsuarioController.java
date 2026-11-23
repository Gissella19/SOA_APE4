package ec.edu.uta.venta_vehiculos_api.controller;

import java.net.URI;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Set;

import org.springframework.data.domain.Sort;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ec.edu.uta.venta_vehiculos_api.exception.ApiError;
import ec.edu.uta.venta_vehiculos_api.model.UsuarioApi;
import ec.edu.uta.venta_vehiculos_api.repository.UsuarioApiRepository;

@RestController
@RequestMapping("/api/usuarios")
public class UsuarioController {

    private static final Set<String> ROLES_VALIDOS = Set.of("CONSULTA", "OPERADOR", "ADMIN");

    private final UsuarioApiRepository repository;
    private final PasswordEncoder passwordEncoder;

    public UsuarioController(
            UsuarioApiRepository repository,
            PasswordEncoder passwordEncoder) {
        this.repository = repository;
        this.passwordEncoder = passwordEncoder;
    }

    @GetMapping
    public List<Map<String, Object>> listar() {
        return repository.findAll(Sort.by(Sort.Direction.ASC, "id"))
                .stream()
                .<Map<String, Object>>map(u -> Map.of(
                        "id", u.getId(),
                        "username", u.getUsername(),
                        "rol", u.getRol()
                ))
                .toList();
    }

    @PostMapping
    public ResponseEntity<Map<String, Object>> crear(
            @RequestBody Map<String, Object> body,
            Authentication auth) {

        String rawUsername = body != null && body.get("username") != null
                ? body.get("username").toString()
                : "";
        String rawPassword = body != null && body.get("password") != null
                ? body.get("password").toString()
                : "";
        String rawRol = body != null && body.get("rol") != null
                ? body.get("rol").toString()
                : "";

        String username = rawUsername.trim();
        String password = rawPassword.trim();
        String rol = rawRol.trim().toUpperCase(Locale.ROOT);

        if (username.isEmpty() || username.length() < 3 || username.length() > 100) {
            throw new ApiError(
                    422,
                    "USUARIO_INVALIDO",
                    "El nombre de usuario debe tener entre 3 y 100 caracteres"
            );
        }

        if (password.isEmpty() || password.length() < 4) {
            throw new ApiError(
                    422,
                    "CONTRASENA_INVALIDA",
                    "La contraseña debe tener al menos 4 caracteres"
            );
        }

        if (!ROLES_VALIDOS.contains(rol)) {
            throw new ApiError(
                    422,
                    "ROL_INVALIDO",
                    "El rol debe ser CONSULTA, OPERADOR o ADMIN"
            );
        }

        boolean esAdmin = auth != null && auth.getAuthorities().stream()
                .anyMatch(a -> "ROLE_ADMIN".equals(a.getAuthority()));
        if ("ADMIN".equals(rol) && !esAdmin) {
            throw new ApiError(
                    403,
                    "SIN_PERMISO",
                    "Solo un usuario ADMIN puede crear cuentas con rol ADMIN"
            );
        }

        if (repository.findByUsername(username).isPresent()) {
            throw new ApiError(
                    409,
                    "USUARIO_DUPLICADO",
                    "El nombre de usuario ya se encuentra registrado"
            );
        }

        UsuarioApi nuevo = new UsuarioApi();
        nuevo.setUsername(username);
        nuevo.setPassword(passwordEncoder.encode(password));
        nuevo.setRol(rol);

        UsuarioApi guardado = repository.saveAndFlush(nuevo);

        Map<String, Object> respuesta = Map.of(
                "id", guardado.getId(),
                "username", guardado.getUsername(),
                "rol", guardado.getRol()
        );

        return ResponseEntity
                .created(URI.create("/api/usuarios/" + guardado.getId()))
                .body(respuesta);
    }
}
