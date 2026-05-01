const express = require('express')
const mysql = require('mysql2/promise')
const session = require('express-session')
const cors = require('cors')

require('dotenv').config({ path: require('path').join(__dirname, '.env') })

const app = express()
const PORT = process.env.PORT || 3000

app.use(express.json())
app.use(cors({ origin: '*', credentials: true }))
app.use(session({
  secret: process.env.SESSION_SECRET || 'changeme',
  resave: false,
  saveUninitialized: false,
  cookie: { secure: false, httpOnly: true, sameSite: 'lax' }
}))

const db = mysql.createPool({
  host: process.env.DB_HOST,
  port: parseInt(process.env.DB_PORT) || 3306,
  database: process.env.DB_NAME,
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  waitForConnections: true,
  connectionLimit: 10,
  ssl: { rejectUnauthorized: false }
})

db.getConnection()
  .then(conn => { console.log('✓ Connexion MySQL établie'); conn.release() })
  .catch(err => console.error('✗ Connexion MySQL échouée:', err.message))

function requireTeacher(req, res, next) {
  const teacherId = parseInt(req.headers['x-teacher-id'])
  if (!teacherId) return res.status(401).json({ message: 'Non connecté.' })
  req.teacherId = teacherId
  next()
}

// POST /api/teacher/login
app.post('/api/teacher/login', async (req, res) => {
  const { login, password } = req.body
  if (!login || !password) return res.status(400).json({ message: 'Champs manquants.' })

  try {
    const [rows] = await db.execute(
      `SELECT u.id, u.login, u.first_name, u.last_name, u.email, u.role_id, t.id AS teacher_id
       FROM user u
       JOIN teacher t ON t.user_id = u.id
       WHERE u.login = ? AND u.password = ? AND u.is_active = 1
       LIMIT 1`,
      [login, password]
    )

    if (rows.length === 0)
      return res.status(401).json({ message: 'Identifiants incorrects ou accès non autorisé.' })

    const u = rows[0]
    if (u.role_id !== 2)
      return res.status(403).json({ message: 'Accès réservé aux professeurs.' })

    req.session.teacher = {
      userId: u.id,
      teacherId: u.teacher_id,
      login: u.login,
      firstName: u.first_name,
      lastName: u.last_name,
      email: u.email,
      roleId: u.role_id
    }

    res.json({ success: true, teacher: req.session.teacher })
  } catch (err) {
    res.status(500).json({ message: 'Erreur serveur: ' + err.message })
  }
})

// POST /api/teacher/logout
app.post('/api/teacher/logout', (req, res) => {
  req.session.destroy(() => res.json({ success: true }))
})

// GET /api/me
app.get('/api/me', (req, res) => {
  if (!req.session.teacher) return res.status(401).json({ message: 'Non connecté.' })
  res.json({ teacher: req.session.teacher })
})

// GET /api/teacher/courses
app.get('/api/teacher/courses', requireTeacher, async (req, res) => {
  try {
    const [rows] = await db.execute(
      `SELECT c.id, c.start_datetime, c.end_datetime,
              sub.name AS subject_name,
              cl.name AS class_name,
              r.name AS room_name
       FROM course c
       JOIN subject sub ON sub.id = c.subject_id
       JOIN class cl ON cl.id = c.class_id
       LEFT JOIN room r ON r.id = c.room_id
       WHERE c.teacher_id = ?
       ORDER BY c.start_datetime DESC
       LIMIT 50`,
      [req.teacherId]
    )
    res.json(rows)
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// GET /api/teacher/courses/:id/students?classId=X
app.get('/api/teacher/courses/:id/students', requireTeacher, async (req, res) => {
  try {
    const courseId = req.params.id
    const classId = req.query.classId ? parseInt(req.query.classId) : null

    let rows
    if (classId) {
      ;[rows] = await db.execute(
        `SELECT s.id, s.first_name, s.last_name,
                a.id AS absence_id, a.is_late
         FROM student s
         LEFT JOIN absence a ON a.student_id = s.id AND a.course_id = ?
         WHERE s.class_id = ?
         ORDER BY s.last_name, s.first_name`,
        [courseId, classId]
      )
    } else {
      ;[rows] = await db.execute(
        `SELECT s.id, s.first_name, s.last_name,
                a.id AS absence_id, a.is_late
         FROM student s
         JOIN class cl ON cl.id = s.class_id
         JOIN course c ON c.class_id = cl.id
         LEFT JOIN absence a ON a.student_id = s.id AND a.course_id = c.id
         WHERE c.id = ?
         ORDER BY s.last_name, s.first_name`,
        [courseId]
      )
    }
    res.json(rows)
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// POST /api/teacher/absences
app.post('/api/teacher/absences', requireTeacher, async (req, res) => {
  const { studentId, courseId, isLate } = req.body
  try {
    const [result] = await db.execute(
      `INSERT INTO absence (student_id, course_id, is_late, is_justified, recorded_at)
       VALUES (?, ?, ?, 0, NOW())`,
      [studentId, courseId, isLate ? 1 : 0]
    )
    res.json({ success: true, absenceId: result.insertId })
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// DELETE /api/teacher/absences/:id
app.delete('/api/teacher/absences/:id', requireTeacher, async (req, res) => {
  try {
    await db.execute('DELETE FROM absence WHERE id = ?', [req.params.id])
    res.json({ success: true })
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// GET /api/teacher/classes
app.get('/api/teacher/classes', requireTeacher, async (req, res) => {
  try {
    const [rows] = await db.execute(
      `SELECT DISTINCT cl.id, cl.name
       FROM course c
       JOIN class cl ON cl.id = c.class_id
       WHERE c.teacher_id = ?
       ORDER BY cl.name`,
      [req.teacherId]
    )
    res.json(rows)
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// GET /api/teacher/rooms
app.get('/api/teacher/rooms', requireTeacher, async (req, res) => {
  try {
    const [rows] = await db.execute('SELECT id, name, capacity, type FROM room WHERE is_active = 1 ORDER BY name')
    res.json(rows)
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// GET /api/teacher/reservations
app.get('/api/teacher/reservations', requireTeacher, async (req, res) => {
  try {
    const [rows] = await db.execute(
      `SELECT rr.id, rr.room_id, r.name AS room_name, rr.start_datetime, rr.end_datetime, rr.reason,
              rr.class_id, cl.name AS class_name
       FROM room_reservation rr
       JOIN room r ON r.id = rr.room_id
       LEFT JOIN class cl ON cl.id = rr.class_id
       WHERE rr.teacher_id = ? AND rr.start_datetime >= CURDATE()
       ORDER BY rr.start_datetime ASC`,
      [req.teacherId]
    )
    res.json(rows)
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// POST /api/teacher/reservations
app.post('/api/teacher/reservations', requireTeacher, async (req, res) => {
  const { roomId, startDatetime, endDatetime, reason, classId } = req.body
  try {
    const [conflict] = await db.execute(
      `SELECT id FROM room_reservation
       WHERE room_id = ? AND start_datetime < ? AND end_datetime > ?`,
      [roomId, endDatetime, startDatetime]
    )
    if (conflict.length > 0)
      return res.status(409).json({ message: 'La salle est déjà réservée sur ce créneau.' })

    await db.execute(
      `INSERT INTO room_reservation (room_id, teacher_id, start_datetime, end_datetime, reason, class_id)
       VALUES (?, ?, ?, ?, ?, ?)`,
      [roomId, req.teacherId, startDatetime, endDatetime, reason || '', classId || null]
    )
    res.json({ success: true })
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// GET /api/teacher/reservations/:id/students
app.get('/api/teacher/reservations/:id/students', requireTeacher, async (req, res) => {
  try {
    const reservationId = req.params.id
    const [rows] = await db.execute(
      `SELECT s.id, s.first_name, s.last_name,
              ra.id AS absence_id, ra.is_late, ra.delay_minutes
       FROM room_reservation rr
       JOIN student s ON s.class_id = rr.class_id
       LEFT JOIN reservation_absence ra ON ra.student_id = s.id AND ra.reservation_id = rr.id
       WHERE rr.id = ? AND rr.teacher_id = ?
       ORDER BY s.last_name, s.first_name`,
      [reservationId, req.teacherId]
    )
    res.json(rows)
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// POST /api/teacher/reservation-absences
app.post('/api/teacher/reservation-absences', requireTeacher, async (req, res) => {
  const { studentId, reservationId, isLate, delayMinutes } = req.body
  try {
    const [result] = await db.execute(
      `INSERT INTO reservation_absence (student_id, reservation_id, is_late, delay_minutes, recorded_at)
       VALUES (?, ?, ?, ?, NOW())`,
      [studentId, reservationId, isLate ? 1 : 0, delayMinutes || 0]
    )
    res.json({ success: true, absenceId: result.insertId })
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// DELETE /api/teacher/reservation-absences/:id
app.delete('/api/teacher/reservation-absences/:id', requireTeacher, async (req, res) => {
  try {
    await db.execute('DELETE FROM reservation_absence WHERE id = ?', [req.params.id])
    res.json({ success: true })
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// GET /api/teacher/rooms/availability?roomId=X&start=...&end=...
app.get('/api/teacher/rooms/availability', requireTeacher, async (req, res) => {
  const { roomId, start, end } = req.query
  if (!roomId || !start || !end)
    return res.status(400).json({ message: 'Paramètres manquants.' })
  try {
    const [conflict] = await db.execute(
      `SELECT id FROM room_reservation
       WHERE room_id = ? AND start_datetime < ? AND end_datetime > ?`,
      [roomId, end, start]
    )
    res.json({ available: conflict.length === 0 })
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

// DELETE /api/teacher/reservations/:id
app.delete('/api/teacher/reservations/:id', requireTeacher, async (req, res) => {
  try {
    await db.execute(
      'DELETE FROM room_reservation WHERE id = ? AND teacher_id = ?',
      [req.params.id, req.teacherId]
    )
    res.json({ success: true })
  } catch (err) {
    res.status(500).json({ message: err.message })
  }
})

app.listen(PORT, () => console.log(`Serveur teacher démarré sur http://localhost:${PORT}`))
