package ec.edu.uta.venta_vehiculos_api.security;

import java.util.List;
import org.springframework.context.annotation.*;
import org.springframework.http.HttpMethod;
import org.springframework.security.config.Customizer;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.http.SessionCreationPolicy;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.security.web.SecurityFilterChain;
import org.springframework.security.web.authentication.UsernamePasswordAuthenticationFilter;
import org.springframework.web.cors.CorsConfiguration;
import org.springframework.web.cors.CorsConfigurationSource;
import org.springframework.web.cors.UrlBasedCorsConfigurationSource;

// Configura el comportamiento de Spring o el mapeo ORM.
@Configuration
public class SecurityConfig {
 // Configura el comportamiento de Spring o el mapeo ORM.
 @Bean
 public org.springframework.boot.web.servlet.FilterRegistrationBean<JwtAuthenticationFilter>
 desactivarJwtServlet(JwtAuthenticationFilter f) {
  var b=new org.springframework.boot.web.servlet.FilterRegistrationBean<>(f);
  b.setEnabled(false); return b;
 }
 // Configura el comportamiento de Spring o el mapeo ORM.
 @Bean
 public org.springframework.boot.web.servlet.FilterRegistrationBean<IntegrationFilter>
 desactivarIntegracionServlet(IntegrationFilter f) {
  var b=new org.springframework.boot.web.servlet.FilterRegistrationBean<>(f);
  b.setEnabled(false); return b;
 }
 // Configura el comportamiento de Spring o el mapeo ORM.
 @Bean public PasswordEncoder passwordEncoder() {
  // Devuelve el resultado del método.
  return new BCryptPasswordEncoder();
 }
 @Bean
 public CorsConfigurationSource corsConfigurationSource() {
  CorsConfiguration config = new CorsConfiguration();
  config.setAllowedOriginPatterns(List.of("*"));
  config.setAllowedMethods(List.of("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"));
  config.setAllowedHeaders(List.of("*"));
  config.setExposedHeaders(List.of("Location"));
  UrlBasedCorsConfigurationSource source = new UrlBasedCorsConfigurationSource();
  source.registerCorsConfiguration("/**", config);
  return source;
 }
 // Configura el comportamiento de Spring o el mapeo ORM.
 @Bean
 // Declara un método o constructor público.
 public SecurityFilterChain securityFilterChain(HttpSecurity http,
   JwtAuthenticationFilter jwt, IntegrationFilter integration)
   throws Exception {
  // Devuelve el resultado del método.
  return http.cors(Customizer.withDefaults())
   .csrf(c -> c.disable())
   .sessionManagement(s -> s.sessionCreationPolicy(
    SessionCreationPolicy.STATELESS))
   .exceptionHandling(e -> e
    .authenticationEntryPoint((q,r,x) -> {
     r.setStatus(401); r.setContentType("application/problem+json");
     r.getWriter().write("{\"title\":\"No autenticado\",\"status\":401}");
    })
    .accessDeniedHandler((q,r,x) -> {
     r.setStatus(403); r.setContentType("application/problem+json");
     r.getWriter().write("{\"title\":\"Sin permiso\",\"status\":403}");
    }))
   .authorizeHttpRequests(a -> a
    .requestMatchers(HttpMethod.OPTIONS, "/**").permitAll()
    .requestMatchers("/oauth/token", "/actuator/health", "/api/vehiculos/instancia").permitAll()
    .requestMatchers("/api/reservas", "/api/reservas/**",
     "/api/vehiculos/*/reservas").hasRole("SERVICIO_VENTAS")
    .requestMatchers("/api/identidad")
     .hasAnyRole("CONSULTA","OPERADOR","ADMIN")
    .requestMatchers(HttpMethod.GET, "/api/usuarios", "/api/usuarios/**")
     .hasAnyRole("CONSULTA","OPERADOR","ADMIN")
    .requestMatchers(HttpMethod.POST, "/api/usuarios", "/api/usuarios/**")
     .hasAnyRole("OPERADOR","ADMIN")
    .requestMatchers(HttpMethod.GET,"/api/vehiculos/**")
     .hasAnyRole("CONSULTA","OPERADOR","ADMIN")
    .requestMatchers(HttpMethod.POST,"/api/vehiculos/**")
     .hasAnyRole("OPERADOR","ADMIN")
    .requestMatchers(HttpMethod.PUT,"/api/vehiculos/**")
     .hasAnyRole("OPERADOR","ADMIN")
    .requestMatchers(HttpMethod.PATCH,"/api/vehiculos/**")
     .hasAnyRole("OPERADOR","ADMIN")
    .requestMatchers(HttpMethod.DELETE,"/api/vehiculos/**")
     .hasRole("ADMIN")
    .anyRequest().denyAll())
   .addFilterBefore(jwt,UsernamePasswordAuthenticationFilter.class)
   .addFilterBefore(integration,JwtAuthenticationFilter.class)
   .build();
 }
}
