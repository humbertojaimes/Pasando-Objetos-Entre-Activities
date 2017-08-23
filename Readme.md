## Pasando datos entre activities (IParcelable, JSON y ISerializable) ##


Hace unas semanas durante una sesión de apoyo del diplomado de Xamarin en la que estuve como asistente, alguien pregunto cómo podía pasar objetos entre dos Activities en lugar de pasar solamente datos primitivos. En esa ocasión yo me comprometí a hacer un post sobre el tema y aun ya pasaron muchas semanas aquí está el post.

En Android no es posible pasar mediante los extras de un Intent elementos que no sean tipos primitivos que nosotros generemos. Al intentar hacerlo obtendremos el siguiente error.


Existen varias manera de poder pasar objetos, cada una con sus ventajas y desventajas. En este post hablaremos de 4, las cuales son las siguientes:

### 1.  Usar elementos estáticos: ###
Esta es la forma más fácil pero la menos limpia y es simplemente crear un elemento publico estático en nuestro primer Activity y desde el segundo Activity lo leemos. A este mecanismo no le daremos mucha importancia debido a que es muy simple y en realidad no estamos pasando nada y aunque parezca increíble hace algunos años cuando comencé con Xamarin y me encontré con esta problemática me encontré con esta solución en muchos foros y grupos sobre Android.

### 2.  Serializar (JSON o XML) ###
Una segunda manera muy simple es serializar nuestro objeto y enviarlo como si fuera una cadena y luego hacer el proceso inverso en el Activity que recibe. Este método es muy simple, mucho más limpio que el método anterior sin embargo es “costoso” en recursos, para el poder de procesamiento actual probablemente ese no sea un gran problema.

### 3.  Crear un objeto Parcelable. ###
Esta es la solución óptima para esta problemática, en Android existe la interfaz Parcelable lo cual en Xamarin se traduce como IParcelable para mantener la nomenclatura de C#. IPacerlable requiere de dos cosas tener un método “WriteToParcel” el cual se encarga de serializar nuestro objeto y tener definido un “Creator” el cuál es el encargado de deserializar el objeto. Este mecanismo es mucho más rápido que serializar a JSON.

### 4.  Serializable ###
En Java existe una interfaz con este nombre y al igual que el caso anterior en Xamarin la tenemos disponible con la nomenclatura de C# como ISerializable, este mecanismo es soportado en Android por temas de compatibilidad con Java pero no es recomendado.

Además, existe una gran diferencia entre usarlo en Java y en Xamarin y es que en Java esta interfaz puede hacer uso de Reflection para serializar el objeto, esto hace que el proceso sea costoso a cambio de no tener que implementar la serialización de forma manual como sucede con IParcelable. En Xamarin no tenemos esta opción y si debemos implementar los métodos de la interfaz de manera manual, los métodos son “WriteObject” y “ReadObject” por esta razón el proceso termina siendo casi igual que con IParcelable y eso nos deja prácticamente sin razones para hacer uso de Serializable para el tema que estamos tratando en este post. 


## Vamos a hacer código ##

Vamos a revisar cómo implementar cada uno de los escenarios y haremos una pequeña y no muy formal prueba de rendimiento para comparar cómo se comporta cada escenario (Solo dejaremos fuera el uso de elementos estáticos debido a que en la mayoría de los casos si no es que en todos no es recomendable usarlo)


Para el ejemplo usare un proyecto en blanco de Xamarin.Android



Y simplemente agregare un Activity nuevo al proyecto llamado “SecondActivity” y lo dejare tal cual como es generado, es decir no le pondré interfaz gráfica.

A lo largo de los ejemplos trabajaremos con esta clase llamada “Employee”

```
public class EmployeeJson
    {

        public string Name
        {
            get;
            set;
        }


        public JobPosition Position
        {
            get;
            set;
        }



        public string Email
        {
            get;
            set;
        }


        public EmployeeJson(JobPosition position, string name, string email)
        {
            Name = name;
            Position = position;
            Email = email;
        }
}

```

### 1. Serialización con JSON ###

Comencemos con el más simple de los escenarios, para este caso la clase “Employee” se mantiene intacta y todo el trabajo lo haremos en el Activity.

Primero debemos instalar el paquete NuGet Json.NET



El proyecto por defecto tiene un botón que al ser presionado modifica un contador, haremos uso de ese mismo botón y su evento clic para el ejemplo.

Al final el manejador queda de esta manera

```
button.Click += delegate
            {
    EmployeeJson myEmployeeJson = new EmployeeJson(JobPosition.Operator, "Alan Lopez", "alan@mail.com");
    Intent secondActivityIntentJson = new Intent(this, typeof(SecondActivity));
    secondActivityIntentJson.PutExtra("employee", Newtonsoft.Json.JsonConvert.SerializeObject(myEmployeeJson));
    StartActivity(secondActivityIntentJson);  
   }; 

```
Con esta solución solo utilizamos 4 líneas de código para enviar el elemento (las líneas de Stopwatch son para medir el tiempo), en el segundo Activity en el método OnCreate recibiremos ese elemento serializado y lo regresamos a ser un objeto con la siguiente lógica

```
EmployeeJson myEmployeeJson = Newtonsoft.Json.JsonConvert.DeserializeObject<EmployeeJson>(Intent.GetStringExtra("employee"));

```
En este caso solo con una línea retornamos los datos a una instancia del objeto.

En mi equipo ejecute este código varias veces, siempre cerrando e iniciando de nuevo la app en modo debug y estos son los resultados. 


Intento | Escritura| Lectura
---------|----------|---------
 1 | 1245 ms | 159 ms
 2 | 1235 ms | 120 ms
 3 | 1189 ms | 177 ms
 4 | 1199 ms | 160 ms
 5 | 1286 ms | 155 ms

En todos los casos supero los 1000ms en serializar y supero los 120 realizar el proceso inverso. 

Como comenté anteriormente es una medición poco formal, simplemente ejecute en el primer emulador (Android 7.1.1 con 2GB de RAM) que se inició y con ese mismo realizare las pruebas a los demás escenarios, todos en las mismas condiciones. (El código está disponible en GitHub al final está el enlace) 


### 2.  IParcelable ###
Para este escenario creare una nueva clase que implementa IParcelable, esta clase tiene las siguietes características:

-   Hereda de Java.Lang.Object, esto debido a que IParcelable implementa las interfaces IJavaObject y también IDisposable, con la clase Object cubrimos los métodos de esas interfaces. 
-   Implementa el método WriteToParcel, en este metodo serializamos el objeto escribiendo una a una las propiedades en un objeto de tipo Parcel. Aquí es importante el orden en que escribimos las propiedades, el orden en que estén escritas es el orden en que deben leerse cuando regeneremos el objeto.
-   Tiene un método público y estático llamado InititalizeCreator que retorna un element de tipo EmployeeParcelableCreator. Este método es el que nos devuelve la instancia del objeto a la hora de deserializarlo. En Android debemos definir un método llamado CREATOR que sea publico estatico que implemente la interfaz “Parcelable.Creator”, en Xamarin usamos el atributo [ExportField("CREATOR")] (dentro del namespace Java.Interop) para indicar que este es el método que requiere IParcelable, también en nuestro caso el método retorna un objeto que implementa la interfaz “IParcelableCreator”, el cual explico más adelante.
*El atributo export sirve para hacer que un método de código manejado pueda ser invocado desde Android, como parámetro le indicamos el nombre con el que lo exponemos. A esto se le llama un método Android Callable Wrapper (ACW)

Así queda definida nuestra clase EmployeeParcelable

```
public class EmployeeParcelable: Java.Lang.Object, IParcelable
    {
        public string Name
        {
            get;
            set;
        }

    
        public JobPosition Position
        {
            get;
            set;
        }

        
    
        public string Email
        {
            get;
            set;
        }

       
        public EmployeeParcelable(JobPosition position, string name,  string email)
        {
            Name = name;
            Position = position;
            Email = email;
        }

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
        {
            dest.WriteInt((int)Position);
            dest.WriteString(Name);
            dest.WriteString(Email);
        }

        [ExportField("CREATOR")]
        public static EmployeeParcelableCreator InititalizeCreator()
        {
            return new EmployeeParcelableCreator();
        }
    }

```

Nuestra otra clase necesaria es EmployeeParcelableCreator, esta tiene las siguientes características:

-   Hereda de Java.Lang.Object por las razones mencionadas en la clase anterior, e implementa IParcelableCreator.
-   Implementa el método CreateFromParcel que debe retornar un objeto del tipo desde el cual es llamado este objeto, para este ejemplo un “EmployeeParcelable”.
-   Es importante que leamos los datos en el orden en que los escribimos en el método WriteToParcel, para este ejemplo los escribi y los leo en el orden en que los espera el constructor del objeto “EmployeeParcelable”
-   La interfaz nos obliga a implementar el método “NewArray” pero ese es un tema que no entra en el alcance de este post por lo cual lo dejaremos con una implementación simple.

```
public class EmployeeParcelableCreator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            return new EmployeeParcelable((JobPosition)source.ReadInt(),source.ReadString(), source.ReadString());
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new Java.Lang.Object[size];
        }


    }

```
Hasta este punto ya escribimos mucho más que en el ejemplo con JSON, ahora falta hacer uso de esta implementación para enviar los datos entre activities

En nuestro MainActivity enviamos de esta manera

```
button.Click += delegate
            {
             EmployeeParcelable myEmployeeParcelable = new EmployeeParcelable(JobPosition.Operator, "Alan Lopez", "alan@mail.com");
                Intent secondActivityIntentParcelable = new Intent(this, typeof(SecondActivity));
                secondActivityIntentParcelable.PutExtra("employee", myEmployeeParcelable);
                StartActivity(secondActivityIntentParcelable); 
   }; 

```

En el Segundo Activity leemos con la siguiente línea 

```
EmployeeParcelable myEmployeeParcelable = (EmployeeParcelable)Intent.GetParcelableExtra("employee"); 

```
Al hacer las pruebas los tiempo de respuesta fueron los siguientes:

Intento | Escritura| Lectura
---------|----------|---------
 1 | 239 ms | 12 ms
 2 | 220 ms | 13 ms
 3 | 253 ms| 14 ms
 4 | 229 ms | 13 ms
 5 | 240 ms| 9 ms


Usando IParcelable el ejemplo se ejecutaba entre 1/4 y 1/5  del tiempo que le tomaba a la solución con JSON

### 3.  ISerializable ###
Para este caso crearemos una clase EmployeeSerializable con las siguientes características.

-   Implementa ISerializable y hereda de Java.Lang.Object esto ultimo por las mismas razones que IParcelable.
-   ISerializable nos obliga a implementar WriteObject y ReadObject al igual que con IParcelable es importante el orden de escritura y de lectura y usamos el atributo Export para exponerlos a Android.
-   Además tenemos unos métodos muy simples para poder reutilizar la escritura y lectura de los datos que serializamos dentro de los Stream que se utilizan para este trabajo.

La clase EmployeeSerializable está definida de este modo

```
public class EmployeeSerializable : Java.Lang.Object, ISerializable
    {

        public EmployeeSerializable(IntPtr handle, JniHandleOwnership transfer)
            : base (handle, transfer)
        {
        }

        [Export("readObject", Throws = new[] {
        typeof (Java.IO.IOException),
        typeof (Java.Lang.ClassNotFoundException)})]
        private void ReadObject(Java.IO.ObjectInputStream source)
        {
            Position = (JobPosition)ReadInt(source);
            Name = ReadNullableString(source);
            Email = ReadNullableString(source);
        }

        [Export("writeObject", Throws = new[] {
        typeof (Java.IO.IOException),
        typeof (Java.Lang.ClassNotFoundException)})]
        private void WriteObject(Java.IO.ObjectOutputStream destination)
        {
            WriteInt(destination, (int)Position);
            WriteNullableString(destination, Name);
            WriteNullableString(destination, Email);
        }

        static void WriteNullableString(Java.IO.ObjectOutputStream dest, string value)
        {
            dest.WriteBoolean(value != null);
            if (value != null)
                dest.WriteUTF(value);
        }

        static void WriteInt(Java.IO.ObjectOutputStream dest, int value)
        {   
            dest.WriteInt(value);
        }

        static string ReadNullableString(Java.IO.ObjectInputStream source)
        {
            if (source.ReadBoolean())
                return source.ReadUTF();
            return null;
        }

        static int ReadInt(Java.IO.ObjectInputStream source)
        {
            return source.ReadInt();
        }

        public string Name
        {
            get;
            set;
        }


        public JobPosition Position
        {
            get;
            set;
        }



        public string Email
        {
            get;
            set;
        }


        public EmployeeSerializable(JobPosition position, string name, string email)
        {
            Name = name;
            Position = position;
            Email = email;
        }


    }

```

Para enviar los datos, en el MainActivity el evento clic quedo de esta manera 

```
button.Click += delegate
            {
             EmployeeSerializable myEmployeeSerializable = new EmployeeSerializable(JobPosition.Operator, "Alan Lopez", "alan@mail.com");
    Intent secondActivityIntentSerializable = new Intent(this, typeof(SecondActivity));
    secondActivityIntentSerializable.PutExtra("employee", myEmployeeSerializable);
    StartActivity(secondActivityIntentSerializable);
   }; 

```
Para leer en el segundo Activity el código es:

```
EmployeeSerializable myEmployeeSerializable = (EmployeeSerializable)Intent.GetSerializableExtra("employee");

```

Los resultados de tiempo de ejecución son los siguientes:


Intento | Escritura| Lectura
---------|----------|---------
 1 | 267 ms | 22 ms
 2 | 253 ms| 29 ms
 3 | 254 ms| 23 ms
 4 | 236 ms | 25 ms
 5 | 240 ms|  21 ms


Se puede considerar que ISerializable fue ligeramente superior en promedio comparado con IParcelable pero los resultados fueron muy similares en la serialización y un muy poco mas lentos en la lectura.


## Conclusión ##

Después de realizar los ejercicios y ver sus tiempos de ejecución podría decir que si es fundamental el rendimiento lo mejor es usar IParcelable el cual es mucho más rápido que serializar objetos en JSON, a cambio de este rendimiento amarramos nuestras clases a algunos elementos propios de Android, podríamos reducir este impacto mediante el uso de herencia sin embargo esto conlleva más tiempo de desarrollo del que por sí solo requiere implementar IParcelable. Por otro lado, el uso de JSON nos permite tener nuestras clases limpias de referencias a elementos de Android además de que es mucho más rápido de implementar. No hay razones para usar ISerialization.

Otro aspecto importante es que realice las pruebas reiniciando la app porque al volver al MainActivity y navegar al SecondActivity con el mismo objeto por segunda vez el tiempo se reducía drásticamente.

Puedes revisar y hacer tus propia comparativa con el código en este enlace, el mismo proyecto contiene los tres escenarios solo es necesario des comentar y comentar los símbolos de compilación en el MainActivity y el SecondActivity según el caso que queramos.





