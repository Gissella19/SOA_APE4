package ec.edu.uta.venta_vehiculos_api.config;

import java.util.Map;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.CommandLineRunner;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Component;

import ec.edu.uta.venta_vehiculos_api.model.UsuarioApi;
import ec.edu.uta.venta_vehiculos_api.repository.UsuarioApiRepository;

@Component
public class DataInitializer implements CommandLineRunner {

    private final UsuarioApiRepository repository;
    private final PasswordEncoder passwordEncoder;
    private final String defaultPassword;

    public DataInitializer(
            UsuarioApiRepository repository,
            PasswordEncoder passwordEncoder,
            @Value("${app.bootstrap.password:cambiar-contrasena-aula-2026}") String defaultPassword) {
        this.repository = repository;
        this.passwordEncoder = passwordEncoder;
        this.defaultPassword = defaultPassword;
    }

    @Override
    public void run(String... args) {
        Map<String, String> usuariosBase = Map.of(
                "usuario_consulta", "CONSULTA",
                "usuario_operador", "OPERADOR",
                "usuario_admin", "ADMIN",
                "operador-test", "OPERADOR",
                "admin-local", "ADMIN"
        );

        usuariosBase.forEach(this::asegurarUsuario);
    }

    private void asegurarUsuario(String username, String rolEsperado) {
        UsuarioApi usuario = repository.findByUsername(username).orElseGet(() -> {
            UsuarioApi nuevo = new UsuarioApi();
            nuevo.setUsername(username);
            nuevo.setRol(rolEsperado);
            nuevo.setPassword(passwordEncoder.encode(defaultPassword));
            return repository.save(nuevo);
        });

        boolean modificado = false;
        if (!rolEsperado.equals(usuario.getRol())) {
            usuario.setRol(rolEsperado);
            modificado = true;
        }
        if (usuario.getPassword() == null || !passwordEncoder.matches(defaultPassword, usuario.getPassword())) {
            usuario.setPassword(passwordEncoder.encode(defaultPassword));
            modificado = true;
        }
        if (modificado) {
            repository.save(usuario);
        }
    }
}
