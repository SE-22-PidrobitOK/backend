FROM nginx:alpine

# Remove default nginx configuration
RUN rm -rf /etc/nginx/conf.d/*

# Copy custom configuration files
COPY ./nginx.conf /etc/nginx/nginx.conf
COPY ./conf.d/services.conf /etc/nginx/conf.d/services.conf

# Create necessary directories
RUN mkdir -p /var/log/nginx

# Expose port 80
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"] 