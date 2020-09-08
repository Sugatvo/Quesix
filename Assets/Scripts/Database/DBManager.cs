public static class DBManager {

    public static string username;
    public static string nombre;
    public static string apellido;
    public static string rol;

    public static bool LoggedIn { get { return username != null; } }

    public static void LogOut()
    {
        username = null;
        nombre = null;
        apellido = null;
        rol = null;
    }


}
